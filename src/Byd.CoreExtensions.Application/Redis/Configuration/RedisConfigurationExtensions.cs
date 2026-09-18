using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Options;
using StackExchange.Redis;
using Volo.Abp;

namespace Byd.CoreExtensions.Redis;

/// <summary>
/// Paylasilan Redis baglantisini servis koleksiyonuna kaydeden uzanti.
/// </summary>
// islevi: Redis seceneklerini host'un Redis bolumunden okur ve tek bir IConnectionMultiplexer kaydeder.
// sistemdeki gorevi: Redis'e yayin ya da abonelik yapan her modulun ayni baglantiyi kullanmasini saglar; baglanti ilk
// kullanimda kurulur, Redis'e dokunmayan host hic baglanmaz.
public static class RedisConfigurationExtensions
{
    // Secenekleri yapilandirmadan okur ve baglantiyi tek ornek olarak kaydeder.
    public static IServiceCollection AddCoreExtensionsRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisConnectionOptions>(options =>
        {
            options.IsEnabled = configuration.GetValue<bool>(RedisConsts.IsEnabledKey);
            options.Configuration = configuration[RedisConsts.ConfigurationKey];
        });

        return services.AddSingleton<IConnectionMultiplexer>(provider =>
            Connect(provider.GetRequiredService<IOptions<RedisConnectionOptions>>().Value));
    }

    // Baglantiyi kurar; Redis acilmamissa ilk kullanimda hata verir, sunucu gecici kapaliysa arka planda yeniden dener.
    private static IConnectionMultiplexer Connect(RedisConnectionOptions options)
    {
        if (!options.IsEnabled || string.IsNullOrWhiteSpace(options.Configuration))
            throw new BusinessException(RedisExceptionCodes.ConnectionNotConfigured);

        var configurationOptions = ConfigurationOptions.Parse(options.Configuration);
        configurationOptions.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(configurationOptions);
    }
}
