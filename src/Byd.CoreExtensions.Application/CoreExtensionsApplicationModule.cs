using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Byd.AntiDuplicate.Middleware;
using Byd.CoreExtensions.ApiKeys;
using Byd.CoreExtensions.Caching;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.EventBus;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.HealthChecks;
using Byd.CoreExtensions.Options;
using Byd.CoreExtensions.RateLimiting;
using Byd.CoreExtensions.SystemLogs;
using Byd.CoreExtensions.Tenants;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
namespace Byd.CoreExtensions;

/// <summary>
/// Application katmaninin modul kompozisyonunu ve calisma zamani kayitlarini kurar.
/// </summary>
// islevi: Application katmaninin ABP modul kompozisyonunu ve servis kayitlarini kurar.
// sistemdeki gorevi: Korumalari, Redis onbellegini, yanit onbellegini ve saglik yoklamalarini kaydeder, host'un modulu pipeline'ina bagladigini acilista dogrular ve arka plan iscilerini devreye alir. Ayar satirini yazma isi host'un cagirdigi UseCoreExtensions uzantisina aittir. Dagitik olaylar RabbitMQ uzerinden tasinir; baglanti bilgisi host'un RabbitMQ bolumunden, Redis baglantisi host'un Redis bolumunden gelir.
[DependsOn(
    typeof(BydAntiDuplicateMiddlewareModule),
    typeof(CoreExtensionsDomainModule),
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpCachingModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpEventBusRabbitMqModule)
    )]
public class CoreExtensionsApplicationModule : AbpModule
{
    // [CachedResponse] tasiyan servisleri, kayit aninda yanit onbellegi interceptor'iyla proxy'ler.
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.OnRegistered(CachedResponseInterceptorRegistrar.RegisterIfNeeded);
    }

    // Rate limit motorunu, API anahtari dogrulamasini ve Mapperly esleyicisini servis koleksiyonuna kaydeder.
    // Mukerrer istek korumasi kendi modulunden konvansiyonel kayitla gelir; burada ayrica kaydedilmez.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Varsayilan politika hicbir olaydan vazgecmez; kalici olarak patlayan tek bir olay
        // kuyrugun basini tikar ve arsivleme kancasi hic islemez. Ustel geri cekilmeli politika
        // hem kuyrugu acik tutar hem de vazgecme anini olusturur.
        // Vazgecme esigi de modulun sorumlulugundadir; politika ile esik ayni yerde durur ki
        // tuketen her host ayni yeniden deneme davranisini beyan etmeden alsin.
        Configure<AbpEventBusBoxesOptions>(options =>
        {
            options.InboxProcessorFailurePolicy = InboxProcessorFailurePolicy.RetryLater;
            options.InboxProcessorMaxRetryCount = EventBoxConsts.InboxProcessorMaxRetryCount;
        });
        // Token'daki tenant claim'i, kullanicinin kendi tenant'ina sabitleyen cerceve cozumleyicisinden once okunur.
        Configure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Insert(0, new ClaimTenantResolveContributor());
        });
        context.Services.AddCoreExtensionsRateLimiter();
        context.Services.AddCoreExtensionsApiKeyValidation();
        context.Services.AddCoreExtensionsRedis(context.Services.GetConfiguration());
        context.Services.AddMapperlyObjectMapper<CoreExtensionsApplicationModule>();
        AddHealthChecks(context);
    }
    // RabbitMQ yoklamasini ve Redis aciksa Redis yoklamasini host'un sectigi seceneklere gore kaydeder; veri depolarinin
    // yoklamalari EntityFrameworkCore modulundedir.
    private static void AddHealthChecks(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<CoreExtensionsHealthCheckOptions>();
        if (!options.IsHealthCheckEnabled)
            return;
        var builder = context.Services.AddHealthChecks();
        if (options.IsRabbitMqHealthCheckEnabled)
            builder.AddCheck<RabbitMqHealthCheck>(HealthCheckConsts.RabbitMqName);
        if (options.IsRedisHealthCheckEnabled && RedisConfigurationExtensions.IsRedisEnabled(context.Services.GetConfiguration()))
            builder.AddCheck<RedisHealthCheck>(HealthCheckConsts.RedisName);
    }
    // Host'un beyanini dogrular ve tazeleme isciyi baslatir; beyan yoksa modulun acilisi burada durur.
    // Isci ilk turunu kendi periyodunda calistirir; acilistaki ilk yukleme UseCoreExtensions
    // uzantisindaki snapshot okumasindan gelir.
    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        EnsureHostConfiguredPipeline(context);
        await context.AddBackgroundWorkerAsync<RateLimitStateWorker>();
        await context.AddBackgroundWorkerAsync<FailedEventInboxProcessWorker>();
        await context.AddBackgroundWorkerAsync<SystemLogWorker>();
    }

    // host UseCoreExtensions cagirmadiysa acilisi durdurur; web olmayan host'lari atlar.
    private static void EnsureHostConfiguredPipeline(ApplicationInitializationContext context)
    {
        var applicationBuilder = context.ServiceProvider
            .GetService<IObjectAccessor<IApplicationBuilder>>()?.Value;
        if (applicationBuilder is null)
            return;
        var pipelineConfigurationState = context.ServiceProvider
            .GetRequiredService<PipelineConfigurationState>();
        if (!pipelineConfigurationState.IsConfigured)
            throw new BusinessException(ModuleSettingExceptionCodes.PipelineNotConfigured);
        
    }
}
