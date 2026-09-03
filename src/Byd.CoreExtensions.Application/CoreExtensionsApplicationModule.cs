using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Byd.AntiDuplicate.Middleware;
using Byd.CoreExtensions.ApiKeys;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.EventBus;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.RateLimiting;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
namespace Byd.CoreExtensions;

/// <summary>
/// Application katmaninin modul kompozisyonunu ve calisma zamani kayitlarini kurar.
/// </summary>
// islevi: Application katmaninin ABP modul kompozisyonunu ve servis kayitlarini kurar.
// sistemdeki gorevi: Korumalari kaydeder, host'un modulu pipeline'ina bagladigini acilista dogrular ve arka plan iscilerini devreye alir. Ayar satirini yazma isi host'un cagirdigi UseCoreExtensions uzantisina aittir.
[DependsOn(
    typeof(BydAntiDuplicateMiddlewareModule),
    typeof(CoreExtensionsDomainModule),
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpCachingModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundWorkersModule)
    )]
public class CoreExtensionsApplicationModule : AbpModule
{
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
        context.Services.AddCoreExtensionsRateLimiter();
        context.Services.AddCoreExtensionsApiKeyValidation();
        context.Services.AddMapperlyObjectMapper<CoreExtensionsApplicationModule>();
    }
    // Host'un beyanini dogrular ve tazeleme isciyi baslatir; beyan yoksa modulun acilisi burada durur.
    // Isci ilk turunu kendi periyodunda calistirir; acilistaki ilk yukleme UseCoreExtensions
    // uzantisindaki snapshot okumasindan gelir.
    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        EnsureHostConfiguredPipeline(context);
        await context.AddBackgroundWorkerAsync<RateLimitStateWorker>();
        await context.AddBackgroundWorkerAsync<FailedEventInboxProcessWorker>();
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
