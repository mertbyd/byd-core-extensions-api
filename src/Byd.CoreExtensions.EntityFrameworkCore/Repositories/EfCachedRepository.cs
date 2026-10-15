using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Settings;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Repositories;

/// <summary>
/// Cache uzerinden okuma yapan repository'ler icin EF Core tabani.
/// </summary>
// islevi: Entity kayitlarinin cache modelleri uzerinden sorgulanmasi icin ortak davranislari saglar; bu repository uzerinden
// yapilan her yazma commit edilince listeyi cache'ten siler. Entity cache'i ayarlarda kapaliysa okumalar tabloya gider.
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

    protected ISettingProvider SettingProvider => LazyServiceProvider.LazyGetRequiredService<ISettingProvider>();

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
    // Cache listesini dondurur; kayit yoksa tabloyu okuyup cache'i doldurur. Entity cache'i kapaliysa tabloyu dogrudan okur.
    public virtual async Task<List<TCacheModel>> CachedGetListAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await SettingProvider.IsTrueAsync(CoreExtensionsSettings.Caching.IsEntityCachingEnabled))
            return await GetListForCachingAsync(cancellationToken);
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
    // Kaydi ekler; liste commit edilince cache'ten silinir.
    public override async Task<TEntity> InsertAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        return await base.InsertAsync(entity, autoSave, cancellationToken);
    }
    // Kayitlari ekler; liste commit edilince cache'ten silinir.
    public override async Task InsertManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        await base.InsertManyAsync(entities, autoSave, cancellationToken);
    }
    // Kaydi gunceller; liste commit edilince cache'ten silinir.
    public override async Task<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        return await base.UpdateAsync(entity, autoSave, cancellationToken);
    }
    // Kayitlari gunceller; liste commit edilince cache'ten silinir.
    public override async Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        await base.UpdateManyAsync(entities, autoSave, cancellationToken);
    }
    // Kaydi siler; liste commit edilince cache'ten silinir.
    public override async Task DeleteAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        await base.DeleteAsync(entity, autoSave, cancellationToken);
    }
    // Kayitlari siler; liste commit edilince cache'ten silinir.
    public override async Task DeleteManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        await base.DeleteManyAsync(entities, autoSave, cancellationToken);
    }
    // Kosula uyan kayitlari siler; liste commit edilince cache'ten silinir.
    public override async Task DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await RemoveCacheListOnCommitAsync(cancellationToken);
        await base.DeleteAsync(predicate, autoSave, cancellationToken);
    }
    // Liste silmesini unit of work'e baglar; commit patlarsa cache dokunulmadan kalir.
    protected virtual Task RemoveCacheListOnCommitAsync(CancellationToken cancellationToken = default)
    {
        return DistributedListCache.RemoveAsync(
            CacheKey,
            considerUow: true,
            token: cancellationToken);
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
