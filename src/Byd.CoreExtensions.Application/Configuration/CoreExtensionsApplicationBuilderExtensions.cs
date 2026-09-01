using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.ApiKeys;
using Byd.CoreExtensions.Data.ModuleSettings;
using Byd.CoreExtensions.RateLimiting;
using Volo.Abp.Data;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions;

/// <summary>
/// Tuketen host'un modulu kendi pipeline'ina bagladigi ve modulun acilis isini tetikledigi uzanti.
/// </summary>
// islevi: Host'un beyanini isaretler, beyan edilen modul adina ait ayar satirini yazar ve bellek goruntulerini yukler.
// sistemdeki gorevi: Modulun tuketen host'a actigi tek acilis kancasi. Modul adi CurrentModuleNameProvider uzerinden host'un Configure beyanindan cozulur; bu cagri yapilmazsa modul satiri hic yazilmaz ve modul acilista durur.
public static class CoreExtensionsApplicationBuilderExtensions
{
    // Host beyanini isaretler ve modulun acilis isini pipeline kurulumu sirasinda calistirir.
    public static IApplicationBuilder UseCoreExtensions(this IApplicationBuilder app)
    {
        app.ApplicationServices.GetRequiredService<PipelineConfigurationState>().MarkConfigured();
        // Pipeline kurulumu senkron oldugu icin acilis isi burada beklenir. Istek yolunda degil,
        // process basina bir kez calisan acilis adimi oldugu icin bloklama kabul edilir.
        InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();
        return app;
    }

    // Once modul satirini yazar, sonra o satirdan beslenen bellek goruntulerini yukler.
    private static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await EnsureModuleSettingAsync(serviceProvider);
        await LoadStateSnapshotsAsync(serviceProvider);
    }

    // Beyan edilen modul satirini kendi unit of work'unde yazar; yazma islemi okuma akisina
    // acik transaction birakmasin diye ayri tutulur. Yazim idempotenttir: mevcut satira dokunmaz,
    // boylece yoneticinin degistirdigi degerler korunur.
    private static async Task EnsureModuleSettingAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();
        await scope.ServiceProvider.GetRequiredService<ModuleSettingDataSeeder>()
            .SeedAsync(new DataSeedContext());
        await unitOfWork.CompleteAsync();
    }

    // Rate limit ayarlarini ve tum API anahtarlarini okuyup bellege yukler. Ilk yukleme basarisiz
    // olsa bile sistem calisir: her iki goruntu de bayat sayilir ve ilk istekte kendini tazeler.
    private static async Task LoadStateSnapshotsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();
        await scope.ServiceProvider.GetRequiredService<RateLimitStateRefresher>().RefreshAsync();
        await scope.ServiceProvider.GetRequiredService<ApiKeyStateRefresher>().RefreshAsync();
        await unitOfWork.CompleteAsync();
    }
}
