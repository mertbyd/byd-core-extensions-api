using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Models.Elasticsearch;
using Byd.CoreExtensions.Querying;
using Volo.Abp.Domain.Entities;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Kayitlari bir Elasticsearch indeksinde tutan ve DataTable sozlesmesiyle hizli arayan repository sozlesmesi.
/// </summary>
// islevi: Indeks yonetimini, tekil ve toplu yazmayi, kimlikle ve alanla okumayi, sayimi ve filtre/siralama/sayfalama ile
// aramayi tanimlar. Indeks adi verilmeyen cagri repository'nin varsayilan indeksini kullanir.
// sistemdeki gorevi: Her modulun arama icin tuttugu kayitlarin tek erisim kapisi; filtre modeli IExtendedRepository ile aynidir,
// ayni liste girdisi iki depoda da calisir. Modul belge tipini dogrudan enjekte eder ya da tabandan turetip indeksini verir.
public interface IElasticsearchRepository<TDocument, in TKey>
    where TDocument : class, IEntity<TKey>
{
    // Kumeye ulasilabiliyorsa true doner; ulasilamazsa hata atmaz.
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);

    // Indeks varsa true doner.
    Task<bool> IndexExistsAsync(string? indexName = null, CancellationToken cancellationToken = default);

    // Indeksi olusturur; kume istegi onaylarsa true doner.
    Task<bool> CreateIndexAsync(string? indexName = null, CancellationToken cancellationToken = default);

    // Indeksi silmeden icindeki tum belgeleri siler.
    Task ClearIndexAsync(string? indexName = null, CancellationToken cancellationToken = default);

    // Belgeyi kimligiyle okur; belge ya da indeks yoksa null doner.
    Task<TDocument?> FindAsync(TKey id, string? indexName = null, CancellationToken cancellationToken = default);

    // Indeksteki belgeleri tek istekte, en fazla tek arama sinirina kadar dondurur.
    Task<List<TDocument>> GetListAsync(string? indexName = null, CancellationToken cancellationToken = default);

    // Indeksteki tum belgeleri parca parca okuyup dondurur; sinir yoktur.
    Task<List<TDocument>> GetAllListAsync(string? indexName = null, CancellationToken cancellationToken = default);

    // Alani verilen degere tam esit belgeleri dondurur.
    Task<List<TDocument>> GetListByAsync<TValue>(
        Expression<Func<TDocument, TValue>> field,
        TValue value,
        string? indexName = null,
        CancellationToken cancellationToken = default);

    // Tarih alani verilen kapali aralikta kalan belgeleri dondurur.
    Task<List<TDocument>> GetListByDateRangeAsync(
        Expression<Func<TDocument, DateTime>> field,
        DateTime startDate,
        DateTime endDate,
        string? indexName = null,
        CancellationToken cancellationToken = default);

    // Indeksteki toplam belge sayisini dondurur; indeks yoksa sifir doner.
    Task<long> GetCountAsync(string? indexName = null, CancellationToken cancellationToken = default);

    // Filtreye uyan en az bir belge varsa true doner.
    Task<bool> AnyAsync<TFiltering>(
        TFiltering filters,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class;

    // Filtre, siralama ve sayfalamayi uygulayip sayfa belgelerini dondurur; usePaging kapaliyken filtreye uyan belgeler
    // tek arama sinirina kadar doner.
    Task<List<TDocument>> GetFilteredListAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        bool usePaging = true,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class;

    // Filtreleri uygulayip sayfalamadan onceki toplam belge sayisini dondurur.
    Task<long> GetFilteredCountAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class;

    // Sayfa belgelerini ve toplam sayiyi tek arama isteginde dondurur.
    Task<ElasticsearchSearchResult<TDocument>> SearchAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class;

    // Ayni aramayi dagitik onbellekten cevaplar; yoksa arar ve sonucu kisa sureligine saklar.
    Task<ElasticsearchSearchResult<TDocument>> CachedSearchAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        string? indexName = null,
        CancellationToken cancellationToken = default)
        where TFiltering : class;

    // Belgeyi kimligiyle indekse yazar; ayni kimlikte belge varsa yerine gecer.
    Task InsertAsync(TDocument document, string? indexName = null, CancellationToken cancellationToken = default);

    // Belgeleri tek istekte indekse yazar; yazilamayan belge varsa hata verir.
    Task InsertManyAsync(
        IEnumerable<TDocument> documents,
        string? indexName = null,
        CancellationToken cancellationToken = default);

    // Var olan belgenin yalniz verilen alanlarini gunceller; belge yoksa hata verir.
    Task UpdateAsync(
        TKey id,
        object partialDocument,
        string? indexName = null,
        CancellationToken cancellationToken = default);

    // Var olan belgeleri tek istekte gunceller; guncellenemeyen belge varsa hata verir.
    Task UpdateManyAsync(
        IEnumerable<TDocument> documents,
        string? indexName = null,
        CancellationToken cancellationToken = default);

    // Belgeyi kimligiyle siler; belge zaten yoksa islem sessizce tamamlanir.
    Task DeleteAsync(TKey id, string? indexName = null, CancellationToken cancellationToken = default);

    // Belgeleri kimlikleriyle tek istekte siler; zaten olmayan belgeler hata sayilmaz.
    Task DeleteManyAsync(
        IEnumerable<TKey> ids,
        string? indexName = null,
        CancellationToken cancellationToken = default);
}
