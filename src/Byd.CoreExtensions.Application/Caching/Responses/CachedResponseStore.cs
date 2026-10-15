using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Providers;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// Onbellege alinan API yanitlarini Redis'te okuyup yazan ve grup grup temizleyen depo.
/// </summary>
// islevi: Anahtari on ek, modul, tenant, grup, organizasyon ve istek ozetinden kurar; yaniti sureli yazar, okur ve bir grubun tum
// anahtarlarini desenle bulup siler. Redis kapaliysa hicbir sey yapmaz; Redis'e ulasilamazsa istek onbelleksiz devam eder.
// sistemdeki gorevi: Yanit onbellegi filtresinin ve grup temizligi isteyen modullerin tek Redis kapisi. Desenle silme anahtar
// biciminin burada sabit olmasina dayanir.
public class CachedResponseStore(
    IAbpLazyServiceProvider lazyServiceProvider,
    IOptions<AbpDistributedCacheOptions> cacheOptions,
    CurrentModuleNameProvider moduleNameProvider,
    ICurrentTenant currentTenant,
    ICurrentOrganization currentOrganization,
    ILogger<CachedResponseStore> logger)
    : ICachedResponseStore, ITransientDependency
{
    // Redis kapaliysa null; yalniz Redis bolumu acik host'ta baglanti kaydedilir.
    private IConnectionMultiplexer? Connection => lazyServiceProvider.LazyGetService<IConnectionMultiplexer>();

    // Redis bagliysa yanit onbellegi kullanilabilir.
    public virtual bool IsAvailable => Connection is not null;

    // Yaniti okur; yoksa ya da Redis'e ulasilamazsa null doner.
    public virtual async Task<string?> FindAsync(string group, string fingerprint, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await Connection!.GetDatabase().StringGetAsync(BuildKey(group, fingerprint));
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception exception) when (exception is RedisException or RedisTimeoutException)
        {
            logger.LogWarning(exception, "Cached response could not be read; the request runs uncached.");
            return null;
        }
    }

    // Yaniti verilen sureyle yazar; Redis'e ulasilamazsa yazmadan gecer.
    public virtual async Task SetAsync(
        string group,
        string fingerprint,
        string content,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Connection!.GetDatabase().StringSetAsync(BuildKey(group, fingerprint), content, duration);
        }
        catch (Exception exception) when (exception is RedisException or RedisTimeoutException)
        {
            logger.LogWarning(exception, "Cached response could not be written.");
        }
    }

    // Yururlukteki tenant'in gruptaki tum yanitlarini siler; Redis kapaliysa islem bostur.
    public virtual async Task RemoveGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        if (Connection is not { } connection)
            return;
        var pattern = BuildGroupPrefix(group) + RedisConsts.PatternAny;
        var keys = new List<RedisKey>();
        foreach (var server in connection.GetServers().Where(server => server is { IsConnected: true, IsReplica: false }))
            await foreach (var key in server.KeysAsync(pattern: pattern).WithCancellation(cancellationToken))
                keys.Add(key);
        if (keys.Count > 0)
            await connection.GetDatabase().KeyDeleteAsync(keys.ToArray());
    }

    // Grup on ekine organizasyonu ve istek ozetini ekler.
    private string BuildKey(string group, string fingerprint)
    {
        return BuildGroupPrefix(group)
               + (currentOrganization.Id?.ToString() ?? CachedResponseConsts.NoScope)
               + CachedResponseConsts.KeySeparator
               + fingerprint;
    }

    // Paylasilan on ek, modul, tenant ve gruptan olusan, desenle silmenin dayandigi anahtar basini kurar.
    private string BuildGroupPrefix(string group)
    {
        return cacheOptions.Value.KeyPrefix
               + string.Join(
                   CachedResponseConsts.KeySeparator,
                   CachedResponseConsts.KeyRoot,
                   moduleNameProvider.Value,
                   currentTenant.Id?.ToString() ?? CachedResponseConsts.NoScope,
                   group)
               + CachedResponseConsts.KeySeparator;
    }
}
