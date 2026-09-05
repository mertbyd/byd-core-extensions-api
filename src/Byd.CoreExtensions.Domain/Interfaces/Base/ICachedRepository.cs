using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Tablonun tamamini cache'te tutup sorgulari cache uzerinden cevaplayan repository sozlesmesi.
/// </summary>
// islevi: Nadiren degisen kayitlari cache modeli olarak saklar, okuma sorgularini veritabanina gitmeden karsilar ve kayit degistiginde cache'i tazeler.
// sistemdeki gorevi: Lookup benzeri tablolarin okuma yolu; ABP'nin dagitik cache altyapisini kullanir, uzerine "komple yukle, bellekten sorgula, degisince temizle" desenini ekler.
public interface ICachedRepository<TEntity, TCacheModel> : IRepository<TEntity>, IScopedDependency
    where TEntity : class, IEntity
    where TCacheModel : class, ICacheModel, IEntity
{
    // Kosulu saglayan ilk kaydi cache uzerinden dondurur.
    Task<TCacheModel?> CachedGetByAsync(
        Expression<Func<TCacheModel, bool>> predicate,
        CancellationToken cancellationToken);
    // Kosulu saglayan kayitlari cache uzerinden dondurur.
    Task<List<TCacheModel>> CachedGetListByAsync(
        Expression<Func<TCacheModel, bool>> predicate,
        CancellationToken cancellationToken);
    // Cache'teki tum kayitlari dondurur; cache bossa once doldurulur.
    Task<List<TCacheModel>> CachedGetListAsync(
        CancellationToken cancellationToken);
    // Kosulu saglayan kayit var mi diye cache uzerinden bakar.
    Task<bool> CachedAnyAsync(
        Expression<Func<TCacheModel, bool>> predicate,
        CancellationToken cancellationToken);
    // Tabloyu okuyup cache'i bastan doldurur.
    Task SetCacheListAsync(
        CancellationToken cancellationToken);
    // Cache'i temizler; bir sonraki okuma tabloyu yeniden yukler.
    Task RemoveCacheListAsync(
        CancellationToken cancellationToken);
    // Entity degisiklik olayi geldiginde cache'i gecersiz kilar; cache tazeligini bu metot korur.
    Task HandleEntityChangedAsync(
        EntityChangedEventData<TEntity> entityChangedEventData,
        CancellationToken cancellationToken);
}
