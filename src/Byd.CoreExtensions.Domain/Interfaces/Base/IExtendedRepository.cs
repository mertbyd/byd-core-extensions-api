using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Querying;
using Volo.Abp.Domain.Entities;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Toplu yazma yeteneklerine ek olarak Faz 3 DataTable sorgulama sozlesmelerini sorguya uygulayan repository sozlesmesi.
/// </summary>
// islevi: Bulk metotlarinin ustune DataTable filtre/siralama/sayfalama ve silme oncesi iliski kontrolu ekler.
// sistemdeki gorevi: Repository zincirinin orta halkasi; IBulkRepository'yi genisletir, ICachedRepository de bunu genisletir. Sik okuma metotlari ABP IRepository'sinden gelir.
public interface IExtendedRepository<TEntity> : IBulkRepository<TEntity>
    where TEntity : class, IEntity
{
    // Filtre, siralama ve sayfalamayi uygulayip sayfa kayitlarini dondurur.
    Task<List<TEntity>> GetFilteredListAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        CancellationToken cancellationToken)
        where TFiltering : class;

    // Filtreleri uygulayip sayfalamadan onceki toplam kayit sayisini dondurur.
    Task<int> GetFilteredCountAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        CancellationToken cancellationToken)
        where TFiltering : class;

    // Kaydin baska tablolardan yabanci anahtarla referans alinip alinmadigini bildirir; silme oncesi guvenlik kontrolu.
    Task<bool> IsDataUsedAsync(
        TEntity entity,
        IEnumerable<Type>? ignoredEntityTypes,
        CancellationToken cancellationToken);
}
