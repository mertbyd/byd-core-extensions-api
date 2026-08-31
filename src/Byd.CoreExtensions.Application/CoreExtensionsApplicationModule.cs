using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.ApiKeys;
using Byd.CoreExtensions.Data.ModuleSettings;
using Byd.CoreExtensions.Idempotency;
using Byd.CoreExtensions.RateLimiting;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
namespace Byd.CoreExtensions;
[DependsOn(
    typeof(CoreExtensionsDomainModule),
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpCachingModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundWorkersModule)
    )]
// islevi: Application katmaninin ABP modul kompozisyonunu ve servis kayitlarini kurar.
// sistemdeki gorevi: Mapperly esleyicisini, rate limit, API anahtari ve anti-duplicate korumasini kaydeder; baslangicta beyan edilen modul satirini yazar, snapshot'lari yukler ve rate limit ayarini TTL periyoduyla tazeleyen isciyi baslatir.
public class CoreExtensionsApplicationModule : AbpModule
{
    // Rate limit motorunu, API anahtari dogrulamasini, anti-duplicate filtresini ve Mapperly esleyicisini servis koleksiyonuna kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddCoreExtensionsRateLimiter();
        context.Services.AddCoreExtensionsApiKeyValidation();
        context.Services.AddMapperlyObjectMapper<CoreExtensionsApplicationModule>();
        context.Services.AddAntiDuplicate();
    }
    // Once beyan edilen modul satirini yazar, sonra bellek goruntulerini yukler ve tazeleme isciyi devreye alir.
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await EnsureModuleSettingAsync(context);
        await LoadStateSnapshotsAsync(context);
        // Isci ilk turunu kendi periyodunda calistirir; acilistaki ilk yukleme yukaridaki snapshot okumasindan gelir.
        await context.AddBackgroundWorkerAsync<RateLimitStateWorker>();
    }

    // Beyan edilen modul satirini kendi unit of work'unde yazar; yazma islemi okuma akisina
    // acik transaction birakmasin diye ayri tutulur. Yazim idempotenttir: mevcut satira dokunmaz,
    // boylece yoneticinin degistirdigi degerler korunur.
    private static async Task EnsureModuleSettingAsync(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();
        await scope.ServiceProvider.GetRequiredService<ModuleSettingDataSeeder>()
            .SeedAsync(new DataSeedContext());
        await unitOfWork.CompleteAsync();
    }

    // Rate limit ayarlarini ve tum API anahtarlarini okuyup bellege yukler. Ilk yukleme basarisiz
    // olsa bile sistem calisir: her iki goruntu de bayat sayilir ve ilk istekte kendini tazeler.
    private static async Task LoadStateSnapshotsAsync(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();
        await scope.ServiceProvider.GetRequiredService<RateLimitStateRefresher>().RefreshAsync();
        await scope.ServiceProvider.GetRequiredService<ApiKeyStateRefresher>().RefreshAsync();
        await unitOfWork.CompleteAsync();
    }
}
