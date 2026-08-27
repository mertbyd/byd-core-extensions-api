using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.ApiKeys;
using Byd.CoreExtensions.RateLimiting;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsDomainModule),
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
    )]
// islevi: Application katmaninin ABP modul kompozisyonunu ve servis kayitlarini kurar.
// sistemdeki gorevi: Mapperly esleyicisini, rate limit ve API anahtari korumasini kaydeder; baslangicta yalnizca ilk rate limit snapshot'ini yukler.
public class CoreExtensionsApplicationModule : AbpModule
{
    // Rate limit motorunu, API anahtari dogrulamasini ve Mapperly esleyicisini servis koleksiyonuna kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddCoreExtensionsRateLimiter();
        context.Services.AddCoreExtensionsApiKeyValidation();
        context.Services.AddMapperlyObjectMapper<CoreExtensionsApplicationModule>();
    }

    // Baslangicta rate limit ayarlarini okuyup bellege yukler.
    // API anahtarlari burada yuklenmez; ApiKeyStore her anahtari ilk kullanildiginda tek tek cozer.
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();
        await scope.ServiceProvider.GetRequiredService<RateLimitStateRefresher>().RefreshAsync();
        await unitOfWork.CompleteAsync();
    }
}
