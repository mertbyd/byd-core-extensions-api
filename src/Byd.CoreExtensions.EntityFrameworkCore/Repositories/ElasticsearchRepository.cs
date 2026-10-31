using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Extensions;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.Elasticsearch;
using Byd.CoreExtensions.Providers;
using Byd.CoreExtensions.Querying;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using CreateIndexRequest = Elastic.Clients.Elasticsearch.IndexManagement.CreateIndexRequest;
using IndexExistsRequest = Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

namespace Byd.CoreExtensions.Repositories;

/// <summary>
/// Kayitlari bir Elasticsearch indeksinde tutan ve DataTable sozlesmesiyle hizli arayan repository.
/// </summary>
// islevi: Indeks yonetimini, yazmayi, okumayi, sayimi ve aramayi istemci uzerinden yapar; filtre/siralama/sayfalamayi
// ElasticsearchRequestExtensions ile kurar. Belge IMultiTenant ya da IMultiOrganization tasiyorsa her okuma, acik veri filtresine
// gore yururlukteki tenant ve organizasyonla sinirlanir.
// sistemdeki gorevi: Her belge tipi icin dogrudan enjekte edilir; ozel indeks ya da ozel sorgu isteyen modul bundan turer,
// IndexName'i ezer ve GetClientAsync ile kendi sorgusunu yazar. Yazmalar indeks yenilenene kadar bekler ki yazilan hemen okunsun.
public class ElasticsearchRepository<TDocument, TKey> : IElasticsearchRepository<TDocument, TKey>, ITransientDependency
    where TDocument : class, IEntity<TKey>
{
    public IAbpLazyServiceProvider LazyServiceProvider { get; set; } = default!;

    // Indeks adi verilmeyen cagrilarin indeksi; Elasticsearch kurali geregi kucuk harflidir.
    protected virtual string IndexName => ElasticsearchConsts.DefaultIndex;

    protected ElasticsearchClientProvider ClientProvider =>
        LazyServiceProvider.LazyGetRequiredService<ElasticsearchClientProvider>();

    protected IDistributedCache<ElasticsearchSearchCacheItem> SearchCache =>
        LazyServiceProvider.LazyGetRequiredService<IDistributedCache<ElasticsearchSearchCacheItem>>();

    protected ICurrentTenant CurrentTenant => LazyServiceProvider.LazyGetRequiredService<ICurrentTenant>();

    protected ICurrentOrganization CurrentOrganization => LazyServiceProvider.LazyGetRequiredService<ICurrentOrganization>();

    protected IDataFilter DataFilter => LazyServiceProvider.LazyGetRequiredService<IDataFilter>();

    // Kumeye ulasilabiliyorsa true doner; ulasilamazsa hata atmaz.
    public virtual async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        try
        {
            return (await client.PingAsync(new PingRequest(), cancellationToken)).IsValidResponse;
        }
        catch (TransportException)
        {
            return false;
        }
    }

    // Indeks varsa true doner.
    public virtual async Task<bool> IndexExistsAsync(string? indexName = null, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        return (await client.Indices.ExistsAsync(new IndexExistsRequest(ResolveIndex(indexName)), cancellationToken)).Exists;
    }

    // Indeksi olusturur; kume istegi onaylarsa true doner.
    public virtual async Task<bool> CreateIndexAsync(string? indexName = null, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        return (await client.Indices.CreateAsync(new CreateIndexRequest(ResolveIndex(indexName)), cancellationToken)).Acknowledged;
    }

    // Indeksi silmeden icindeki tum belgeleri siler; indeks yoksa islem sessizce tamamlanir.
    public virtual async Task ClearIndexAsync(string? indexName = null, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        await client.DeleteByQueryAsync(
            new DeleteByQueryRequest(ResolveIndex(indexName))
            {
                Query = new MatchAllQuery(),
                IgnoreUnavailable = true,
                AllowNoIndices = true,
                Refresh = true
            },
            cancellationToken);
    }

    // Belgeyi kimligiyle okur; belge, indeks ya da kapsam disi kayit icin null doner.
    public virtual async Task<TDocument?> FindAsync(
        TKey id,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var condition = ElasticsearchRequestExtensions.BuildTermQuery<TDocument>(nameof(IEntity<TKey>.Id), id);
        var documents = await SearchDocumentsAsync(condition, size: 1, indexName, cancellationToken);
        return documents.FirstOrDefault();
    }

    // Indeksteki belgeleri tek istekte, en fazla tek arama sinirina kadar dondurur.
    public virtual Task<List<TDocument>> GetListAsync(string? indexName = null, CancellationToken cancellationToken = default)
    {
        return SearchDocumentsAsync(null, ElasticsearchConsts.MaxDocumentCount, indexName, cancellationToken);
    }

    // Indeksteki tum belgeleri scroll ile parca parca okur ve baglami kapatir.
    public virtual async Task<List<TDocument>> GetAllListAsync(string? indexName = null, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        var request = CreateSearchRequest(indexName, null);
        request.Size = ElasticsearchConsts.MaxDocumentCount;
        request.Scroll = ElasticsearchConsts.ScrollKeepAlive;
        var response = await client.SearchAsync<TDocument>(request, cancellationToken);
        var documents = response.Documents.ToList();
        var scrollId = response.ScrollId;
        var pageCount = response.Documents.Count;
        while (scrollId is not null && pageCount > 0)
        {
            var page = await client.ScrollAsync<TDocument>(
                new ScrollRequest(scrollId) { Scroll = ElasticsearchConsts.ScrollKeepAlive },
                cancellationToken);
            documents.AddRange(page.Documents);
            scrollId = page.ScrollId;
            pageCount = page.Documents.Count;
        }
        await ClearScrollAsync(client, scrollId, cancellationToken);
        return documents;
    }

    // Alani verilen degere tam esit belgeleri dondurur.
    public virtual Task<List<TDocument>> GetListByAsync<TValue>(
        Expression<Func<TDocument, TValue>> field,
        TValue value,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var condition = ElasticsearchRequestExtensions.BuildTermQuery<TDocument>(
            ElasticsearchRequestExtensions.ResolvePropertyPath(field), value);
        return SearchDocumentsAsync(condition, ElasticsearchConsts.MaxDocumentCount, indexName, cancellationToken);
    }

    // Tarih alani verilen kapali aralikta kalan belgeleri dondurur.
    public virtual Task<List<TDocument>> GetListByDateRangeAsync(
        Expression<Func<TDocument, DateTime>> field,
        DateTime startDate,
        DateTime endDate,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var condition = ElasticsearchRequestExtensions.BuildDateRangeQuery<TDocument>(
            ElasticsearchRequestExtensions.ResolvePropertyPath(field), startDate, endDate);
        return SearchDocumentsAsync(condition, ElasticsearchConsts.MaxDocumentCount, indexName, cancellationToken);
    }

    // Kapsamdaki toplam belge sayisini dondurur; indeks yoksa sifir doner.
    public virtual Task<long> GetCountAsync(string? indexName = null, CancellationToken cancellationToken = default)
    {
        return CountAsync(BuildScopedQuery(null), indexName, cancellationToken);
    }

    // Filtreye uyan en az bir belge varsa true doner.
    public virtual async Task<bool> AnyAsync<TFiltering>(
        TFiltering filters,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class
    {
        var query = ElasticsearchRequestExtensions.BuildQuery<TDocument, TFiltering>(filters, BuildScopeConditions());
        return await CountAsync(query, indexName, cancellationToken) > 0;
    }

    // Filtre, siralama ve sayfalamayi uygulayip sayfa belgelerini dondurur; usePaging kapaliyken filtreye uyan belgeler
    // tek arama sinirina kadar doner.
    public virtual async Task<List<TDocument>> GetFilteredListAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        bool usePaging = true,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class
    {
        var request = CreateFilteredRequest(pagingProperty, indexName);
        if (!usePaging)
        {
            request.From = 0;
            request.Size = ElasticsearchConsts.MaxDocumentCount;
        }
        var client = await GetClientAsync(cancellationToken);
        return (await client.SearchAsync<TDocument>(request, cancellationToken)).Documents.ToList();
    }

    // Filtreleri uygulayip sayfalamadan onceki toplam belge sayisini dondurur.
    public virtual Task<long> GetFilteredCountAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class
    {
        var query = ElasticsearchRequestExtensions.BuildQuery<TDocument, TFiltering>(pagingProperty.Filters, BuildScopeConditions());
        return CountAsync(query, indexName, cancellationToken);
    }

    // Sayfa belgelerini ve toplam sayiyi tek arama isteginde dondurur.
    public virtual async Task<ElasticsearchSearchResult<TDocument>> SearchAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class
    {
        var request = CreateFilteredRequest(pagingProperty, indexName);
        request.TrackTotalHits = new TrackHits(true);
        var client = await GetClientAsync(cancellationToken);
        var response = await client.SearchAsync<TDocument>(request, cancellationToken);
        return new ElasticsearchSearchResult<TDocument> { Items = response.Documents.ToList(), TotalCount = response.Total };
    }

    // Ayni aramayi dagitik onbellekten cevaplar; yoksa arar ve sonucu kisa sureligine saklar. Anahtar indeksten, filtre
    // tipinden, istegin tamamindan ve organizasyon kapsamindan kurulur; tenant'i ABP'nin onbellegi ekler.
    public virtual async Task<ElasticsearchSearchResult<TDocument>> CachedSearchAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class
    {
        var cacheItem = await SearchCache.GetOrAddAsync(
            CreateSearchCacheKey(pagingProperty, indexName),
            async () => new ElasticsearchSearchCacheItem
            {
                Json = JsonSerializer.Serialize(
                    await SearchAsync(pagingProperty, indexName, cancellationToken),
                    ElasticsearchClientProvider.DocumentSerializerOptions)
            },
            () => new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ElasticsearchConsts.SearchCacheDuration },
            token: cancellationToken);
        return JsonSerializer.Deserialize<ElasticsearchSearchResult<TDocument>>(
            cacheItem!.Json,
            ElasticsearchClientProvider.DocumentSerializerOptions)!;
    }

    // Belgeyi kimligiyle indekse yazar; ayni kimlikte belge varsa yerine gecer.
    public virtual async Task InsertAsync(
        TDocument document,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        await client.IndexAsync(
            document,
            index => index.Index(ResolveIndex(indexName)).Refresh(Refresh.WaitFor),
            cancellationToken);
    }

    // Belgeleri tek istekte indekse yazar; yazilamayan belge varsa sayisiyla birlikte hata verir.
    public virtual async Task InsertManyAsync(
        IEnumerable<TDocument> documents,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        EnsureBulkSucceeded(await client.BulkAsync(
            bulk => bulk.Index(ResolveIndex(indexName)).Refresh(Refresh.WaitFor).IndexMany(documents),
            cancellationToken));
    }

    // Var olan belgenin yalniz verilen alanlarini gunceller; belge yoksa istemci hatasi yukari tasinir.
    public virtual async Task UpdateAsync(
        TKey id,
        object partialDocument,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        await client.UpdateAsync<TDocument, object>(
            ResolveIndex(indexName),
            id!.ToString()!,
            update => update.Doc(partialDocument).Refresh(Refresh.WaitFor),
            cancellationToken);
    }

    // Var olan belgeleri tek istekte gunceller; guncellenemeyen belge varsa sayisiyla birlikte hata verir.
    public virtual async Task UpdateManyAsync(
        IEnumerable<TDocument> documents,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        EnsureBulkSucceeded(await client.BulkAsync(
            bulk => bulk.Index(ResolveIndex(indexName))
                .Refresh(Refresh.WaitFor)
                .UpdateMany(documents, (update, document) => update.Id(document.Id!.ToString()!).Doc(document)),
            cancellationToken));
    }

    // Belgeyi kimligiyle siler; belge zaten yoksa islem sessizce tamamlanir.
    public virtual async Task DeleteAsync(TKey id, string? indexName = null, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        try
        {
            await client.DeleteAsync(
                new DeleteRequest(ResolveIndex(indexName), id!.ToString()!) { Refresh = Refresh.WaitFor },
                cancellationToken);
        }
        catch (TransportException exception) when (exception.ApiCallDetails?.HttpStatusCode == (int)HttpStatusCode.NotFound)
        {
            // Belge ya da indeks zaten yok; silmenin istedigi son durum budur.
        }
    }

    // Belgeleri kimlikleriyle tek istekte siler; zaten olmayan belgeler hata sayilmaz.
    public virtual async Task DeleteManyAsync(
        IEnumerable<TKey> ids,
        string? indexName = null,
        CancellationToken cancellationToken = default)
    {
        var index = ResolveIndex(indexName);
        var client = await GetClientAsync(cancellationToken);
        EnsureBulkSucceeded(await client.BulkAsync(
            bulk => bulk.Refresh(Refresh.WaitFor).DeleteMany(index, ids.Select(id => new Id(id!.ToString()!))),
            cancellationToken));
    }

    // Ozel sorgu yazan alt repository'nin kullandigi istemci; ayarlar degistiyse yeni kurulmus olani doner.
    protected virtual Task<ElasticsearchClient> GetClientAsync(CancellationToken cancellationToken = default)
    {
        return ClientProvider.GetClientAsync(cancellationToken);
    }

    // Belge kapsam isareti tasiyorsa acik veri filtresine gore tenant ve organizasyon kosullarini kurar.
    protected virtual IReadOnlyCollection<Query> BuildScopeConditions()
    {
        var conditions = new List<Query>();
        if (typeof(IMultiTenant).IsAssignableFrom(typeof(TDocument)) && DataFilter.IsEnabled<IMultiTenant>())
            conditions.Add(ElasticsearchRequestExtensions.BuildTermQuery<TDocument>(nameof(IMultiTenant.TenantId), CurrentTenant.Id));
        if (typeof(IMultiOrganization).IsAssignableFrom(typeof(TDocument)) && DataFilter.IsEnabled<IMultiOrganization>())
            conditions.Add(ElasticsearchRequestExtensions.BuildTermQuery<TDocument>(
                nameof(IMultiOrganization.OrganizationId), CurrentOrganization.Id));
        return conditions;
    }

    // Kapsam kosullarini verilen ek kosulla birlestirir.
    private Query BuildScopedQuery(Query? condition)
    {
        var conditions = new List<Query>(BuildScopeConditions());
        if (condition is not null)
            conditions.Add(condition);
        return ElasticsearchRequestExtensions.Combine(conditions);
    }

    // Kapsamli kosula uyan belgeleri verilen sayiya kadar dondurur.
    private async Task<List<TDocument>> SearchDocumentsAsync(
        Query? condition,
        int size,
        string? indexName,
        CancellationToken cancellationToken = default)
    {
        var request = CreateSearchRequest(indexName, condition);
        request.Size = size;
        var client = await GetClientAsync(cancellationToken);
        return (await client.SearchAsync<TDocument>(request, cancellationToken)).Documents.ToList();
    }

    // Filtre, siralama ve sayfalamasi uygulanmis arama istegini kurar.
    private SearchRequest CreateFilteredRequest<TFiltering>(IDataTablePagingProperty<TFiltering> pagingProperty, string? indexName)
        where TFiltering : class
    {
        return new SearchRequest(ResolveIndex(indexName))
            {
                IgnoreUnavailable = true,
                AllowNoIndices = true,
                Query = ElasticsearchRequestExtensions.BuildQuery<TDocument, TFiltering>(pagingProperty.Filters, BuildScopeConditions())
            }
            .ApplySorting<TDocument, TKey>(pagingProperty.Sorting, typeof(TFiltering))
            .ApplyPaging(pagingProperty.Page, pagingProperty.PageSize);
    }

    // Indeks yoksa bos sonuc donen, kapsamli kosulla kurulmus arama istegini kurar.
    private SearchRequest CreateSearchRequest(string? indexName, Query? condition)
    {
        return new SearchRequest(ResolveIndex(indexName))
        {
            IgnoreUnavailable = true,
            AllowNoIndices = true,
            Query = BuildScopedQuery(condition)
        };
    }

    // Sorguya uyan belge sayisini dondurur; indeks yoksa sifir doner.
    private async Task<long> CountAsync(Query query, string? indexName, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);
        var request = new CountRequest(ResolveIndex(indexName)) { IgnoreUnavailable = true, AllowNoIndices = true, Query = query };
        return (await client.CountAsync(request, cancellationToken)).Count;
    }

    // Arama isteginin tamamini ozetleyen onbellek anahtarini kurar.
    private string CreateSearchCacheKey<TFiltering>(IDataTablePagingProperty<TFiltering> pagingProperty, string? indexName)
        where TFiltering : class
    {
        var request = JsonSerializer.Serialize(pagingProperty, pagingProperty.GetType());
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request)));
        return string.Join(
            CachedResponseConsts.KeySeparator,
            ResolveIndex(indexName),
            typeof(TFiltering).FullName,
            CurrentOrganization.Id?.ToString() ?? CachedResponseConsts.NoScope,
            hash);
    }

    // Verilen indeks adini, yoksa repository'nin varsayilan indeksini dondurur.
    private string ResolveIndex(string? indexName)
    {
        return string.IsNullOrWhiteSpace(indexName) ? IndexName : indexName;
    }

    // Scroll baglami acik kaldiysa kapatir.
    private static async Task ClearScrollAsync(ElasticsearchClient client, ScrollId? scrollId, CancellationToken cancellationToken = default)
    {
        if (scrollId is not null)
            await client.ClearScrollAsync(new ClearScrollRequest { ScrollId = scrollId }, cancellationToken);
    }

    // Toplu istekte reddedilen belge varsa sayisiyla birlikte hata verir.
    private static void EnsureBulkSucceeded(BulkResponse response)
    {
        if (response.Errors)
            throw new BusinessException(ElasticsearchExceptionCodes.BulkWriteFailed)
                .WithData(ElasticsearchConsts.FailedItemCountDataKey, response.ItemsWithErrors.Count());
    }
}
