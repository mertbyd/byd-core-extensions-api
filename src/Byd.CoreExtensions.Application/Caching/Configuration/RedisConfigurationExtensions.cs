using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Consts;
using StackExchange.Redis;
using Volo.Abp.Caching;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// Dagitik onbellegi Redis'e baglayan uzanti.
/// </summary>
// islevi: Host'un Redis bolumu acik ve baglanti dizesi doluysa tek paylasilan baglanti kurar; ABP'nin dagitik onbellegini, yanit
// onbellegini ve saglik yoklamasini bu baglantiya baglar, ayni Redis'i paylasan sistemleri anahtar on ekiyle ayirir. Bolum yoksa
// ya da kapaliysa ABP'nin bellek ici onbellegi kalir.
// sistemdeki gorevi: Redis kurulumunun tek yeri; tuketen host yalniz appsettings'e Redis bolumunu koyar. Baglanti ilk kullanimda
// acilir ve kopsa da host'u durdurmaz, kendini yeniden kurar.
public static class RedisConfigurationExtensions
{
    // Redis aciksa paylasilan baglantiyi, dagitik onbellegi ve anahtar on ekini kaydeder.
    public static IServiceCollection AddCoreExtensionsRedis(this IServiceCollection services, IConfiguration configuration)
    {
        if (!IsRedisEnabled(configuration))
            return services;

        var options = ConfigurationOptions.Parse(configuration[RedisConsts.ConfigurationKey]!);
        options.AbortOnConnectFail = false;
        var connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        services.AddSingleton(_ => connection.Value);
        // Kayit ABP'nin bellek ici onbelleginden sonra eklendigi icin dagitik onbellek Redis'e gecer.
        services.AddStackExchangeRedisCache(redis => redis.ConnectionMultiplexerFactory = () => Task.FromResult(connection.Value));
        services.Configure<AbpDistributedCacheOptions>(cache =>
            cache.KeyPrefix = configuration[RedisConsts.KeyPrefixKey] ?? string.Empty);
        return services;
    }

    // Redis bolumu acik ve baglanti dizesi doluysa true doner.
    public static bool IsRedisEnabled(IConfiguration configuration)
    {
        return bool.TryParse(configuration[RedisConsts.IsEnabledKey], out var isEnabled)
               && isEnabled
               && !string.IsNullOrWhiteSpace(configuration[RedisConsts.ConfigurationKey]);
    }
}
