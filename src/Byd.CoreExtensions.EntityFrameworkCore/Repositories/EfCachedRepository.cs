using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.Repositories;

/// <summary>
/// Cache uzerinden okuma yapan repository'ler icin EF Core tabani.
/// </summary>
// islevi: Entity kayitlarinin cache modelleri uzerinden sorgulanmasi icin ortak davranislari saglar.
// sistemdeki gorevi: ICachedRepository sozlesmesinin EF Core tabani; somut repository'ler cache doldurma ve model donusum ayrintilarini tamamlar.
public abstract class EfCachedRepository<TDbContext, TEntity, TCacheModel>
    : EfCoreRepository<TDbContext, TEntity>, ICachedRepository<TEntity, TCacheModel>
    where TDbContext : DbContext, IEfCoreDbContext
    where TEntity : class, IEntity
    where TCacheModel : class, ICacheModel, IEntity
{
    protected IDistributedCache<IEnumerable<TCacheModel>> DistributedListCache { get; }

    protected int CacheDurationInSeconds { get; }

    protected virtual string CacheKey =>
        typeof(TCacheModel).FullName ?? typeof(TCacheModel).Name;

    protected EfCachedRepository(
        IDbContextProvider<TDbContext> dbContextProvider,
        IDistributedCache<IEnumerable<TCacheModel>> distributedListCache,
        int cacheDurationInSeconds)
        : base(dbContextProvider)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cacheDurationInSeconds);

        DistributedListCache = distributedListCache;
        CacheDurationInSeconds = cacheDurationInSeconds;
    }
    // Kosulu saglayan ilk cache modelini dondurur.
    public virtual async Task<TCacheModel?> CachedGetByAsync(
        Expression<Func<TCacheModel, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var cacheModels = await CachedGetListAsync(cancellationToken);
        return cacheModels.FirstOrDefault(predicate.Compile());
    }
    // Kosulu saglayan cache modellerini dondurur.
    public virtual async Task<List<TCacheModel>> CachedGetListByAsync(
        Expression<Func<TCacheModel, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var cacheModels = await CachedGetListAsync(cancellationToken);
        return cacheModels.Where(predicate.Compile()).ToList();
    }
    // Cache listesini dondurur; kayit yoksa tabloyu okuyup cache'i doldurur.
    public virtual async Task<List<TCacheModel>> CachedGetListAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheModels = await DistributedListCache.GetOrAddAsync(
            CacheKey,
            async () => await GetListForCachingAsync(cancellationToken),
            CreateCacheEntryOptions,
            token: cancellationToken);
        return cacheModels?.ToList() ?? new List<TCacheModel>();
    }
    // Kosulu saglayan en az bir cache modeli olup olmadigini bildirir.
    public virtual async Task<bool> CachedAnyAsync(
        Expression<Func<TCacheModel, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var cacheModels = await CachedGetListAsync(cancellationToken);
        return cacheModels.Any(predicate.Compile());
    }
    // Tabloyu okuyup cache listesini bastan yazar.
    public virtual async Task SetCacheListAsync(CancellationToken cancellationToken = default)
    {
        var cacheModels = await GetListForCachingAsync(cancellationToken);
        await DistributedListCache.SetAsync(
            CacheKey,
            cacheModels,
            CreateCacheEntryOptions(),
            token: cancellationToken);
    }
    // Mevcut tenant'a ait cache listesini siler.
    public virtual async Task RemoveCacheListAsync(CancellationToken cancellationToken = default)
    {
        await DistributedListCache.RemoveAsync(
            CacheKey,
            token: cancellationToken);
    }
    // Entity degisikliginde cache'i gecersiz kilar; sonraki okuma listeyi yeniden doldurur.
    public virtual Task HandleEntityChangedAsync(
        EntityChangedEventData<TEntity> entityChangedEventData,
        CancellationToken cancellationToken = default)
    {
        return RemoveCacheListAsync(cancellationToken);
    }
    // Tenant ve diger ABP filtrelerini koruyarak tabloyu cache model listesine donusturur.
    protected virtual async Task<List<TCacheModel>> GetListForCachingAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await base.GetListAsync(
            includeDetails: true,
            cancellationToken: cancellationToken);
        return entities.Select(MapToCacheModel).ToList();
    }
    // Cache omrunu ayarlar; int.MaxValue kalici cache girdisi anlamina gelir.
    protected virtual DistributedCacheEntryOptions CreateCacheEntryOptions()
    {
        var options = new DistributedCacheEntryOptions();
        if (CacheDurationInSeconds != int.MaxValue)
        {
            options.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromSeconds(CacheDurationInSeconds);
        }
        return options;
    }

    // Entity'yi cache'te tutulacak sade modele donusturme kararini somut repository'ye birakir.
    protected abstract TCacheModel MapToCacheModel(TEntity source);
}
