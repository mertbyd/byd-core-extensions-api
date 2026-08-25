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
    // UTC'ye normalize eden tarih ayristirma stili.
    private const DateTimeStyles UtcDateStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
    // Filtre modeli tipi basina cikarilan izin listesi; her sorguda yansimayi tekrarlamayi onler.
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<(PropertyInfo Property, DataTableAttribute Attribute)>> FilterFieldCache = new();
    // Tip basina ozel metin cozucu; listede olmayan tip Convert.ChangeType'a duser.
    private static readonly IReadOnlyDictionary<Type, Func<string, object>> ValueParsers = new Dictionary<Type, Func<string, object>>
    {
        [typeof(Guid)] = text => Guid.Parse(text),
        [typeof(DateTime)] = text => DateTime.Parse(text, CultureInfo.InvariantCulture, UtcDateStyles),
        [typeof(DateTimeOffset)] = text => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, UtcDateStyles),
        [typeof(DateOnly)] = text => DateOnly.Parse(text, CultureInfo.InvariantCulture),
        [typeof(TimeOnly)] = text => TimeOnly.Parse(text, CultureInfo.InvariantCulture),
        [typeof(TimeSpan)] = text => TimeSpan.Parse(text, CultureInfo.InvariantCulture),
        [typeof(byte[])] = text => Convert.FromBase64String(text),
    };
    // Dolu filtre alanlarini AndAlso ile birlestirip sorguya Where olarak uygular.
    public static IQueryable<TEntity> ApplyFilters<TEntity, TFiltering>(
        this IQueryable<TEntity> query,
        TFiltering? filters)
        where TFiltering : class
    {
        if (filters == null)
            return query;
        var parameter = Expression.Parameter(typeof(TEntity), DataTableQueryConsts.EntityParameterName);
        Expression? body = null;
        foreach (var (property, attribute) in GetFilterFields(typeof(TFiltering)))
        {
            if (property.GetValue(filters) is not DataTableFilterItem { Operator: not null } item)
                continue;
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
                continue;

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
        return query.Skip(ResolveSkipCount(page, pageSize)).Take(pageSize);
    }
    // Sayfalamayi dogrular ve atlanacak kayit sayisini tasma olmadan hesaplar.
    private static int ResolveSkipCount(int page, int pageSize)
    {
        if (page < DataTablePaging.FirstPage)
            throw PagingException(GeneralExceptionCodes.InvalidPage, DataTableQueryConsts.PageDataKey, page);
        if (pageSize < 1 || pageSize > DataTablePaging.MaxPageSize)
            throw PagingException(GeneralExceptionCodes.InvalidPageSize, DataTableQueryConsts.PageSizeDataKey, pageSize);
        var skip = ((long)page - DataTablePaging.FirstPage) * pageSize;
        if (skip > int.MaxValue)
            throw PagingException(GeneralExceptionCodes.InvalidPage, DataTableQueryConsts.PageDataKey, page);
        return (int)skip;
    }
    // Sayfalama hatasini kararli kod ve veri anahtariyla kurar.
    private static BusinessException PagingException(string errorCode, string dataKey, int value)
    {
        return new BusinessException(errorCode).WithData(dataKey, value);
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
                Expression.Equal(member, ParameterOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.NotEqual =>
                Expression.NotEqual(member, ParameterOf(item.Value, parseType, member.Type)),
            DataTableFilterOperator.GreaterThan =>
                BuildOrderComparison(ExpressionType.GreaterThan, member, item.Value, parseType, DataTableFilterOperator.GreaterThan),
            DataTableFilterOperator.GreaterThanOrEqual =>
                BuildOrderComparison(ExpressionType.GreaterThanOrEqual, member, item.Value, parseType, DataTableFilterOperator.GreaterThanOrEqual),
            DataTableFilterOperator.LessThan =>
                BuildOrderComparison(ExpressionType.LessThan, member, item.Value, parseType, DataTableFilterOperator.LessThan),
            DataTableFilterOperator.LessThanOrEqual =>
                BuildOrderComparison(ExpressionType.LessThanOrEqual, member, item.Value, parseType, DataTableFilterOperator.LessThanOrEqual),
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
                .WithData(DataTableQueryConsts.OperatorDataKey, item.Operator!)
        };
    }
    // Siralama karsilastirmasini kurar; tip bu operatoru desteklemiyorsa ham hatayi temiz UnsupportedFilterOperator'a cevirir.
    private static Expression BuildOrderComparison(
        ExpressionType comparison,
        Expression member,
        string? raw,
        Type parseType,
        string @operator)
    {
        var value = ParameterOf(raw, parseType, member.Type);
        try
        {
            return Expression.MakeBinary(comparison, member, value);
        }
        catch (InvalidOperationException exception)
        {
            throw new BusinessException(GeneralExceptionCodes.UnsupportedFilterOperator, innerException: exception)
                .WithData(DataTableQueryConsts.OperatorDataKey, @operator)
                .WithData(DataTableQueryConsts.TargetTypeDataKey, member.Type.FullName ?? member.Type.Name);
        }
    }
    // Value ve SecondValue arasindaki kapali araligi kurar.
    private static Expression BuildBetween(Expression member, DataTableFilterItem item, Type parseType)
    {
        var lower = BuildOrderComparison(ExpressionType.GreaterThanOrEqual, member, item.Value, parseType, DataTableFilterOperator.Between);
        var upper = BuildOrderComparison(ExpressionType.LessThanOrEqual, member, item.SecondValue, parseType, DataTableFilterOperator.Between);
        return Expression.AndAlso(lower, upper);
    }
    // Metin alaninda gecme kosulunu kurar; string olmayan alanda temiz hata verir.
    private static Expression BuildContains(Expression member, string? value)
    {
        if (member.Type != typeof(string))
            throw new BusinessException(GeneralExceptionCodes.UnsupportedFilterOperator)
                .WithData(DataTableQueryConsts.OperatorDataKey, DataTableFilterOperator.Contains)
                .WithData(DataTableQueryConsts.TargetTypeDataKey, member.Type.FullName ?? member.Type.Name);

        var method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        var isNotNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
        var contains = Expression.Call(member, method, BoxedAccess(value ?? string.Empty, typeof(string)));
        return Expression.AndAlso(isNotNull, contains);
    }
    // Verilen degerlerden birine esitlik kosulunu kurar.
    private static Expression BuildIn(Expression member, IEnumerable<string>? values, Type parseType)
    {
        var listType = typeof(List<>).MakeGenericType(member.Type);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var raw in values ?? Enumerable.Empty<string>())
            list.Add(ConvertValue(raw, parseType));
        var contains = typeof(Enumerable)
            .GetMethods()
            .First(method => method.Name == nameof(Enumerable.Contains)
                             && method.GetParameters().Length == 2)
            .MakeGenericMethod(member.Type);
        return Expression.Call(contains, BoxedAccess(list, listType), member);
    }
    // Nullable veya referans tipli alan icin null esitligi/esitsizligi kurar.
    private static Expression BuildNullComparison(Expression member, bool equalsNull)
    {
        if (member.Type.IsValueType && Nullable.GetUnderlyingType(member.Type) == null)
            return Expression.Constant(!equalsNull);
        var comparison = Expression.Equal(member, Expression.Constant(null, member.Type));
        return equalsNull ? comparison : Expression.Not(comparison);
    }
    // Noktali property yolunu entity parametresi uzerinde uye erisim zincirine cevirir.
    private static Expression BuildMemberAccess(Expression parameter, string propertyPath)
    {
        Expression current = parameter;
        foreach (var part in propertyPath.Split(DataTableQueryConsts.PropertyPathSeparator))
            current = Expression.Property(current, part);
        return current;
    }
    // Ham metni cozup filtre degerini EF'in parametreye cevirdigi kapsayici uzerinden ifadeye baglar.
    private static Expression ParameterOf(string? raw, Type parseType, Type memberType)
    {
        var access = BoxedAccess(ConvertValue(raw, parseType), null);
        return access.Type == memberType ? access : Expression.Convert(access, memberType);
    }
    // Degeri sabit bir kapsayici uzerindeki uye erisimine cevirir; EF Core bunu yakalanmis parametre gibi gorup SQL parametresi uretir.
    private static Expression BoxedAccess(object value, Type? valueType)
    {
        var boxType = typeof(QueryValueBox<>).MakeGenericType(valueType ?? value.GetType());
        var box = Activator.CreateInstance(boxType, value)!;
        return Expression.Property(Expression.Constant(box, boxType), nameof(QueryValueBox<object>.Value));
    }
    // Ham metni hedef tipe culture-invariant cevirir; beklenen donusum hatasi lokalize InvalidFilterValue olur.
    private static object ConvertValue(string? raw, Type targetType)
    {
        var text = raw ?? string.Empty;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        try
        {
            return ParseValue(text, underlying);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            throw CreateInvalidFilterValueException(text, underlying, exception);
        }
    }

    // Metni tipe gore cozer: enum, kayitli ozel cozucu ya da Convert.ChangeType.
    private static object ParseValue(string text, Type underlying)
    {
        if (underlying.IsEnum)
            return Enum.Parse(underlying, text, ignoreCase: true);

        return ValueParsers.TryGetValue(underlying, out var parser)
            ? parser(text)
            : Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
    }

    // Beklenen donusum hatasini kararli ve lokalize edilebilir BusinessException'a cevirir.
    private static BusinessException CreateInvalidFilterValueException(
        string value,
        Type targetType,
        Exception innerException)
    {
        return new BusinessException(GeneralExceptionCodes.InvalidFilterValue, innerException: innerException)
            .WithData(DataTableQueryConsts.ValueDataKey, value)
            .WithData(DataTableQueryConsts.TargetTypeDataKey, targetType.FullName ?? targetType.Name);
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
