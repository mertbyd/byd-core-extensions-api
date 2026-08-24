using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Querying;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Byd.CoreExtensions.Extensions;

/// <summary>
/// DataTable sorgulama sozlesmelerini IQueryable uzerine filtre, siralama ve sayfalama olarak uygulayan uzanti metotlari.
/// </summary>
// islevi: [DataTable] ile isaretlenmis alanlari izin listesi uzerinden entity yoluna cevirip Where/OrderBy/Skip-Take ekler.
// sistemdeki gorevi: Faz 3 sorgulama altyapisinin calisan motoru; repository bu metotlari cagirir, property adi hicbir zaman dogrudan istemciden gelmez.
public static class QueryableExtensions
{
    // Filtre modeli tipi basina cikarilan izin listesi; her sorguda yansimayi tekrarlamayi onler.
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<(PropertyInfo Property, DataTableAttribute Attribute)>> FilterFieldCache = new();

    // Dolu filtre alanlarini AndAlso ile birlestirip sorguya Where olarak uygular.
    public static IQueryable<TEntity> ApplyFilters<TEntity, TFiltering>(
        this IQueryable<TEntity> query,
        TFiltering? filters)
        where TFiltering : class
    {
        if (filters == null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(TEntity), DataTableQueryConsts.EntityParameterName);
        Expression? body = null;

        foreach (var (property, attribute) in GetFilterFields(typeof(TFiltering)))
        {
            if (property.GetValue(filters) is not DataTableFilterItem { Operator: not null } item)
            {
                continue;
            }

            var condition = BuildCondition(parameter, attribute, item);
            body = body == null ? condition : Expression.AndAlso(body, condition);
        }

        return body == null
            ? query
            : query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
    }

    // Siralama adimlarini izin listesi uzerinden uygular ve sonuna Id tie-breaker'i ekler.
    public static IQueryable<TEntity> ApplySorting<TEntity, TKey>(
        this IQueryable<TEntity> query,
        List<DataTableSorting>? sorting,
        Type filteringType)
        where TEntity : class, IEntity<TKey>
    {
        var fields = GetFilterFields(filteringType);
        IOrderedQueryable<TEntity>? ordered = null;

        foreach (var step in sorting ?? Enumerable.Empty<DataTableSorting>())
        {
            var field = fields.FirstOrDefault(
                item => string.Equals(item.Attribute.Name, step.Field, StringComparison.OrdinalIgnoreCase));

            // Izin listesinde olmayan alanla siralama yapilmaz.
            if (field.Attribute == null)
            {
                continue;
            }

            ordered = ApplyOrderStep(query, ordered, field.Attribute.PropertyPath, step.Descending);
        }

        // Deterministik sayfalama icin son anahtar her zaman Id'dir.
        return ordered == null
            ? query.OrderBy(entity => entity.Id)
            : ordered.ThenBy(entity => entity.Id);
    }

    // Sayfa numarasi ve boyutuna gore atlanacak ve alinacak kayit sayisini uygular.
    public static IQueryable<TEntity> ApplyPaging<TEntity>(
        this IQueryable<TEntity> query,
        int page,
        int pageSize)
    {
        if (page < DataTablePaging.FirstPage)
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidPage)
                .WithData("page", page);
        }

        if (pageSize < 1 || pageSize > DataTablePaging.MaxPageSize)
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidPageSize)
                .WithData("pageSize", pageSize);
        }

        var skipValue = ((long)page - DataTablePaging.FirstPage) * pageSize;
        if (skipValue > int.MaxValue)
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidPage)
                .WithData("page", page);
        }

        return query.Skip((int)skipValue).Take(pageSize);
    }

    // Filtre modeli tipinin izin listesini cikarir; sonucu tip basina cache'ler.
    private static IReadOnlyList<(PropertyInfo Property, DataTableAttribute Attribute)> GetFilterFields(Type filteringType)
    {
        return FilterFieldCache.GetOrAdd(filteringType, static type => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => (property, attribute: property.GetCustomAttribute<DataTableAttribute>()))
            .Where(entry => entry.attribute != null
                            && entry.property.PropertyType == typeof(DataTableFilterItem))
            .Select(entry => (entry.property, entry.attribute!))
            .ToList());
    }

    // Tek bir filtre alani icin operatore uygun Expression kosulunu uretir.
    private static Expression BuildCondition(
        ParameterExpression parameter,
        DataTableAttribute attribute,
        DataTableFilterItem item)
    {
        var member = BuildMemberAccess(parameter, attribute.PropertyPath);
        var parseType = attribute.TargetType ?? Nullable.GetUnderlyingType(member.Type) ?? member.Type;

        return item.Operator switch
        {
            DataTableFilterOperator.Equal =>
                Expression.Equal(member, ConstantOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.NotEqual =>
                Expression.NotEqual(member, ConstantOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.GreaterThan =>
                Expression.GreaterThan(member, ConstantOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.GreaterThanOrEqual =>
                Expression.GreaterThanOrEqual(member, ConstantOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.LessThan =>
                Expression.LessThan(member, ConstantOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.LessThanOrEqual =>
                Expression.LessThanOrEqual(member, ConstantOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.Contains =>
                BuildContains(member, item.Value),
            DataTableFilterOperator.Between =>
                BuildBetween(member, item, parseType),
            DataTableFilterOperator.In =>
                BuildIn(member, item.Values, parseType),
            DataTableFilterOperator.IsNull =>
                BuildNullComparison(member, equalsNull: true),
            DataTableFilterOperator.IsNotNull =>
                BuildNullComparison(member, equalsNull: false),
            _ => throw new BusinessException(GeneralExceptionCodes.UnsupportedFilterOperator)
                .WithData("operator", item.Operator!)
        };
    }

    // Value ve SecondValue arasindaki kapali araligi kurar.
    private static Expression BuildBetween(Expression member, DataTableFilterItem item, Type parseType)
    {
        var lower = Expression.GreaterThanOrEqual(member, ConstantOf(item.Value, parseType, member.Type));
        var upper = Expression.LessThanOrEqual(member, ConstantOf(item.SecondValue, parseType, member.Type));

        return Expression.AndAlso(lower, upper);
    }

    // Metin alaninda gecme kosulunu kurar.
    private static Expression BuildContains(Expression member, string? value)
    {
        if (member.Type != typeof(string))
        {
            throw new BusinessException(GeneralExceptionCodes.UnsupportedFilterOperator)
                .WithData("operator", DataTableFilterOperator.Contains)
                .WithData("targetType", member.Type.FullName ?? member.Type.Name);
        }

        var method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        var isNotNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
        var contains = Expression.Call(member, method, Expression.Constant(value ?? string.Empty));

        return Expression.AndAlso(isNotNull, contains);
    }

    // Verilen degerlerden birine esitlik kosulunu kurar.
    private static Expression BuildIn(Expression member, IEnumerable<string>? values, Type parseType)
    {
        var listType = typeof(List<>).MakeGenericType(member.Type);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var raw in values ?? Enumerable.Empty<string>())
        {
            list.Add(ConvertValue(raw, parseType));
        }

        var contains = typeof(Enumerable)
            .GetMethods()
            .First(method => method.Name == nameof(Enumerable.Contains)
                             && method.GetParameters().Length == 2)
            .MakeGenericMethod(member.Type);

        return Expression.Call(contains, Expression.Constant(list, listType), member);
    }

    // Nullable veya referans tipli alan icin null esitligi/esitsizligi kurar.
    private static Expression BuildNullComparison(Expression member, bool equalsNull)
    {
        if (member.Type.IsValueType && Nullable.GetUnderlyingType(member.Type) == null)
        {
            return Expression.Constant(!equalsNull);
        }

        var comparison = Expression.Equal(member, Expression.Constant(null, member.Type));
        return equalsNull ? comparison : Expression.Not(comparison);
    }

    // Noktali property yolunu entity parametresi uzerinde uye erisim zincirine cevirir.
    private static Expression BuildMemberAccess(Expression parameter, string propertyPath)
    {
        Expression current = parameter;

        foreach (var part in propertyPath.Split('.'))
        {
            current = Expression.Property(current, part);
        }

        return current;
    }

    // Ham metni parseType ile cozup uye tipine uyan sabit ifadeye donusturur.
    private static Expression ConstantOf(string? raw, Type parseType, Type memberType)
    {
        var value = ConvertValue(raw, parseType);
        Expression constant = Expression.Constant(value);

        return constant.Type == memberType ? constant : Expression.Convert(constant, memberType);
    }

    // Ham metni hedef tipe culture-invariant kurallarla cevirir.
    private static object ConvertValue(string? raw, Type targetType)
    {
        var text = raw ?? string.Empty;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlying.IsEnum)
            {
                return Enum.Parse(underlying, text, ignoreCase: true);
            }

            if (underlying == typeof(Guid))
            {
                return Guid.Parse(text);
            }

            if (underlying == typeof(DateTime))
            {
                return DateTime.Parse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }

            if (underlying == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }

            if (underlying == typeof(DateOnly))
            {
                return DateOnly.Parse(text, CultureInfo.InvariantCulture);
            }

            if (underlying == typeof(TimeOnly))
            {
                return TimeOnly.Parse(text, CultureInfo.InvariantCulture);
            }

            if (underlying == typeof(TimeSpan))
            {
                return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
            }

            if (underlying == typeof(byte[]))
            {
                return Convert.FromBase64String(text);
            }

            return Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
        }
        catch (FormatException exception)
        {
            throw CreateInvalidFilterValueException(text, underlying, exception);
        }
        catch (InvalidCastException exception)
        {
            throw CreateInvalidFilterValueException(text, underlying, exception);
        }
        catch (OverflowException exception)
        {
            throw CreateInvalidFilterValueException(text, underlying, exception);
        }
        catch (ArgumentException exception)
        {
            throw CreateInvalidFilterValueException(text, underlying, exception);
        }
    }

    // Beklenen donusum hatasini kararli ve lokalize edilebilir BusinessException'a cevirir.
    private static BusinessException CreateInvalidFilterValueException(
        string value,
        Type targetType,
        Exception innerException)
    {
        return new BusinessException(
                GeneralExceptionCodes.InvalidFilterValue,
                innerException: innerException)
            .WithData("value", value)
            .WithData("targetType", targetType.FullName ?? targetType.Name);
    }

    // Tek bir siralama adimini OrderBy/ThenBy olarak dinamik uye tipiyle uygular.
    private static IOrderedQueryable<TEntity> ApplyOrderStep<TEntity>(
        IQueryable<TEntity> query,
        IOrderedQueryable<TEntity>? ordered,
        string propertyPath,
        bool descending)
    {
        var parameter = Expression.Parameter(typeof(TEntity), DataTableQueryConsts.EntityParameterName);
        var member = BuildMemberAccess(parameter, propertyPath);
        var keySelector = Expression.Lambda(member, parameter);

        var methodName = ordered == null
            ? (descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
            : (descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(TEntity), member.Type },
            (ordered ?? query).Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(call);
    }
}
