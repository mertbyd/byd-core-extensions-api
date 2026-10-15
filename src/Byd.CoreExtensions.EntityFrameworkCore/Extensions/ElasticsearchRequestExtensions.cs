using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Querying;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Byd.CoreExtensions.Extensions;

/// <summary>
/// DataTable sorgulama sozlesmelerini Elasticsearch sorgularina, siralamasina ve sayfalamasina cevirir.
/// </summary>
// islevi: [DataTable] izin listesindeki alanlari belge alan adlarina cevirir, her operatoru karsilik gelen Elasticsearch sorgusuna
// donusturur; alan-deger, tarih araligi ve kapsam kosullarini da ayni alan cozumuyle kurar.
// sistemdeki gorevi: QueryableExtensions'in arama indeksi karsiligi; ayni filtre modeli, ayni deger cozumu ve ayni hata kodlari
// iki depoda da gecerlidir. Tam metin ve bulanik arama yalniz burada vardir.
public static class ElasticsearchRequestExtensions
{
    // Joker aramada kullanicinin yazdigi ozel karakterlerin kacis bicimleri.
    private static readonly (string Character, string Escaped)[] WildcardEscapes = [("\\", "\\\\"), ("*", "\\*"), ("?", "\\?")];

    // Dolu filtre alanlarinin ve kapsam kosullarinin birlikte saglanmasi gereken tek sorgusunu kurar; kosul yoksa tum belgeler
    // eslesir.
    public static Query BuildQuery<TDocument, TFiltering>(TFiltering? filters, IReadOnlyCollection<Query> scopeConditions)
        where TFiltering : class
    {
        var conditions = new List<Query>(scopeConditions);
        foreach (var (property, attribute) in QueryableExtensions.GetFilterFields(typeof(TFiltering)))
        {
            if (filters == null || property.GetValue(filters) is not DataTableFilterItem { Operator: not null } item)
                continue;
            conditions.Add(BuildCondition(typeof(TDocument), attribute, item));
        }
        return Combine(conditions);
    }

    // Kosullari tek bool sorgusunda toplar; kosul yoksa tum belgeler eslesir.
    public static Query Combine(IReadOnlyCollection<Query> conditions)
    {
        return conditions.Count == 0
            ? new MatchAllQuery()
            : new BoolQuery { Filter = conditions.ToList() };
    }

    // Alanin verilen degere tam esit oldugu kosulu kurar; deger bossa alanin hic olmadigi kosulu kurar.
    public static Query BuildTermQuery<TDocument>(string propertyPath, object? value)
    {
        var memberType = ResolveMemberType(typeof(TDocument), propertyPath);
        var field = ResolveField(propertyPath, Nullable.GetUnderlyingType(memberType) ?? memberType, useKeyword: true);
        return value is null
            ? new BoolQuery { MustNot = [new ExistsQuery(field)] }
            : new TermQuery(field, ToFieldValue(ToDocumentValue(value)));
    }

    // Tarih alaninin verilen kapali aralikta kaldigi kosulu kurar.
    public static Query BuildDateRangeQuery<TDocument>(string propertyPath, DateTime startDate, DateTime endDate)
    {
        var field = ResolveField(propertyPath, typeof(DateTime), useKeyword: false);
        return new UntypedRangeQuery { Field = field, Gte = ToDocumentValue(startDate), Lte = ToDocumentValue(endDate) };
    }

    // Uye ifadesini noktali property yoluna cevirir; ifade dogrudan uye zinciri degilse hata verir.
    public static string ResolvePropertyPath<TDocument, TValue>(Expression<Func<TDocument, TValue>> field)
    {
        var parts = new Stack<string>();
        var current = field.Body is UnaryExpression { NodeType: ExpressionType.Convert } conversion ? conversion.Operand : field.Body;
        while (current is MemberExpression member)
        {
            parts.Push(member.Member.Name);
            current = member.Expression;
        }
        if (current is not ParameterExpression || parts.Count == 0)
            throw new ArgumentException(field.ToString(), nameof(field));
        return string.Join(DataTableQueryConsts.PropertyPathSeparator, parts);
    }

    // Siralama adimlarini izin listesi uzerinden uygular ve sonuna Id tie-breaker'i ekler.
    public static SearchRequest ApplySorting<TDocument, TKey>(
        this SearchRequest request,
        List<DataTableSorting>? sorting,
        Type filteringType)
        where TDocument : IEntity<TKey>
    {
        var fields = QueryableExtensions.GetFilterFields(filteringType);
        var sortOptions = new List<SortOptions>();
        foreach (var step in sorting ?? Enumerable.Empty<DataTableSorting>())
        {
            var field = fields.FirstOrDefault(
                item => string.Equals(item.Attribute.Name, step.Field, StringComparison.OrdinalIgnoreCase));

            // Izin listesinde olmayan alanla siralama yapilmaz.
            if (field.Attribute == null)
                continue;

            sortOptions.Add(BuildSort(typeof(TDocument), field.Attribute.PropertyPath, step.Descending));
        }
        // Deterministik sayfalama icin son anahtar her zaman Id'dir.
        sortOptions.Add(BuildSort(typeof(TDocument), nameof(IEntity<TKey>.Id), descending: false));
        request.Sort = sortOptions;
        return request;
    }

    // Sayfa numarasi ve boyutuna gore atlanacak ve alinacak belge sayisini uygular.
    public static SearchRequest ApplyPaging(this SearchRequest request, int page, int pageSize)
    {
        request.From = QueryableExtensions.ResolveSkipCount(page, pageSize);
        request.Size = pageSize;
        return request;
    }

    // Tek bir filtre alani icin operatore uygun Elasticsearch kosulunu uretir.
    private static Query BuildCondition(Type documentType, DataTableAttribute attribute, DataTableFilterItem item)
    {
        var memberType = ResolveMemberType(documentType, attribute.PropertyPath);
        var parseType = attribute.TargetType ?? Nullable.GetUnderlyingType(memberType) ?? memberType;
        var field = ResolveField(attribute.PropertyPath, parseType, useKeyword: true);

        return item.Operator switch
        {
            DataTableFilterOperator.Equal =>
                new TermQuery(field, TermValueOf(item.Value, parseType)),
            DataTableFilterOperator.NotEqual =>
                new BoolQuery { MustNot = [new TermQuery(field, TermValueOf(item.Value, parseType))] },
            DataTableFilterOperator.GreaterThan =>
                new UntypedRangeQuery { Field = field, Gt = RangeValueOf(item.Value, parseType) },
            DataTableFilterOperator.GreaterThanOrEqual =>
                new UntypedRangeQuery { Field = field, Gte = RangeValueOf(item.Value, parseType) },
            DataTableFilterOperator.LessThan =>
                new UntypedRangeQuery { Field = field, Lt = RangeValueOf(item.Value, parseType) },
            DataTableFilterOperator.LessThanOrEqual =>
                new UntypedRangeQuery { Field = field, Lte = RangeValueOf(item.Value, parseType) },
            DataTableFilterOperator.Contains =>
                BuildContains(field, item.Value, parseType, item.Operator),
            DataTableFilterOperator.NotContains =>
                new BoolQuery { MustNot = [BuildContains(field, item.Value, parseType, item.Operator)] },
            DataTableFilterOperator.Match =>
                BuildMatch(attribute.PropertyPath, item.Value, parseType, item.Operator, fuzziness: null),
            DataTableFilterOperator.Fuzzy =>
                BuildMatch(attribute.PropertyPath, item.Value, parseType, item.Operator, new Fuzziness(ElasticsearchConsts.AutoFuzziness)),
            DataTableFilterOperator.Between =>
                new UntypedRangeQuery
                {
                    Field = field,
                    Gte = RangeValueOf(item.Value, parseType),
                    Lte = RangeValueOf(item.SecondValue, parseType)
                },
            DataTableFilterOperator.In =>
                new TermsQuery(field, new TermsQueryField(
                    (item.Values ?? []).Select(value => TermValueOf(value, parseType)).ToList())),
            DataTableFilterOperator.IsNull =>
                new BoolQuery { MustNot = [new ExistsQuery(field)] },
            DataTableFilterOperator.IsNotNull =>
                new ExistsQuery(field),
            _ => throw new BusinessException(GeneralExceptionCodes.UnsupportedFilterOperator)
                .WithData(DataTableQueryConsts.OperatorDataKey, item.Operator!)
        };
    }

    // Metin alaninda buyuk/kucuk harf duyarsiz gecme kosulunu joker sorgusuyla kurar.
    private static Query BuildContains(Field field, string? value, Type parseType, string @operator)
    {
        EnsureText(parseType, @operator);
        var escaped = WildcardEscapes.Aggregate(
            value ?? string.Empty,
            (text, escape) => text.Replace(escape.Character, escape.Escaped));
        return new WildcardQuery(field)
        {
            Value = $"{ElasticsearchConsts.WildcardAny}{escaped}{ElasticsearchConsts.WildcardAny}",
            CaseInsensitive = true
        };
    }

    // Metin alaninin analiz edilmis haline kelime bazli eslesme kurar; bulaniklik verilirse yazim hatasina tolerans tanir.
    private static Query BuildMatch(string propertyPath, string? value, Type parseType, string @operator, Fuzziness? fuzziness)
    {
        EnsureText(parseType, @operator);
        return new MatchQuery(ResolveField(propertyPath, parseType, useKeyword: false), value ?? string.Empty)
        {
            Operator = Operator.And,
            Fuzziness = fuzziness
        };
    }

    // Metin aramasi gerektiren operator metin olmayan alana gelirse temiz hata verir.
    private static void EnsureText(Type parseType, string @operator)
    {
        if (parseType != typeof(string))
            throw new BusinessException(GeneralExceptionCodes.UnsupportedFilterOperator)
                .WithData(DataTableQueryConsts.OperatorDataKey, @operator)
                .WithData(DataTableQueryConsts.TargetTypeDataKey, parseType.FullName ?? parseType.Name);
    }

    // Tek bir siralama adimini belge alani uzerinde kurar.
    private static SortOptions BuildSort(Type documentType, string propertyPath, bool descending)
    {
        var memberType = ResolveMemberType(documentType, propertyPath);
        var field = ResolveField(propertyPath, Nullable.GetUnderlyingType(memberType) ?? memberType, useKeyword: true);
        return new FieldSort { Field = field, Order = descending ? SortOrder.Desc : SortOrder.Asc };
    }

    // Noktali property yolunun belge tipindeki son uyesinin tipini bulur.
    private static Type ResolveMemberType(Type documentType, string propertyPath)
    {
        return propertyPath
            .Split(DataTableQueryConsts.PropertyPathSeparator)
            .Aggregate(documentType, (type, part) => type.GetProperty(part)!.PropertyType);
    }

    // Property yolunu belgenin camelCase alan adina cevirir; tam eslesme istenen metin tiplerinde keyword alt alanini secer.
    private static Field ResolveField(string propertyPath, Type valueType, bool useKeyword)
    {
        var name = string.Join(
            DataTableQueryConsts.PropertyPathSeparator,
            propertyPath.Split(DataTableQueryConsts.PropertyPathSeparator).Select(JsonNamingPolicy.CamelCase.ConvertName));
        var isWrittenAsText = valueType == typeof(string) || valueType == typeof(Guid) || valueType.IsEnum;
        return useKeyword && isWrittenAsText ? name + ElasticsearchConsts.KeywordSuffix : name;
    }

    // Ham metni hedef tipe cozup terim karsilastirmasinin degerine cevirir.
    private static FieldValue TermValueOf(string? raw, Type parseType)
    {
        return ToFieldValue(RangeValueOf(raw, parseType));
    }

    // Ham metni hedef tipe cozer ve belgede yazildigi JSON bicimine indirger.
    private static object RangeValueOf(string? raw, Type parseType)
    {
        return ToDocumentValue(QueryableExtensions.ConvertValue(raw, parseType));
    }

    // Degeri belgede yazildigi JSON bicimine indirger: mantiksal, tam sayi, ondalik ya da metin.
    private static object ToDocumentValue(object value)
    {
        return value switch
        {
            bool => value,
            Enum => value.ToString()!,
            sbyte or byte or short or ushort or int or uint or long => Convert.ToInt64(value, CultureInfo.InvariantCulture),
            float or double or decimal => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)!
        };
    }

    // JSON bicimine indirgenmis degeri terim sorgusunun deger tipine cevirir.
    private static FieldValue ToFieldValue(object value)
    {
        return value switch
        {
            bool boolean => FieldValue.Boolean(boolean),
            long number => FieldValue.Long(number),
            double number => FieldValue.Double(number),
            var text => FieldValue.String((string)text)
        };
    }
}
