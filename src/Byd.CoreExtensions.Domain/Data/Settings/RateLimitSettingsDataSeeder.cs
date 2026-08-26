using System.Globalization;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
namespace Byd.CoreExtensions.Settings;
// islevi: Rate limit global varsayilanlarini AbpSettings global scope'una idempotent olarak seed'ler.
// sistemdeki gorevi: ABP IDataSeeder calistiginda ayar satiri yoksa varsayilani yazar; admin degistirdiyse mevcut degeri korur.
public class RateLimitSettingsDataSeeder
    : IDataSeedContributor,
        ITransientDependency
{
    private readonly ISettingManager _settingManager;
    public RateLimitSettingsDataSeeder(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }
    // Uc rate limit ayarini global scope'ta eksikse varsayilan degeriyle tamamlar; tekrar kosum degeri ezmez.
    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        await EnsureGlobalAsync(
            CoreExtensionsSettings.RateLimitDefaultAlgorithm,
            RateLimitConsts.DefaultAlgorithm);
        await EnsureGlobalAsync(
            CoreExtensionsSettings.RateLimitDefaultPermitLimit,
            RateLimitConsts.DefaultPermitLimit.ToString(CultureInfo.InvariantCulture));
        await EnsureGlobalAsync(
            CoreExtensionsSettings.RateLimitDefaultWindowSeconds,
            RateLimitConsts.DefaultWindowSeconds.ToString(CultureInfo.InvariantCulture));
    }
    // Ayar global scope'a yazilmamissa varsayilan degeri satir olarak yazar; mevcut degeri korur.
    // fallback: false zorunludur; varsayilan okuma tanimdaki degeri dondurur ve hicbir zaman bos gelmez.
    // forceToSet: true zorunludur; ABP, yazilan deger fallback zincirinden gelenle ayni oldugunda
    // satiri gereksiz sayip yazmaz ve seeder sessizce hicbir sey uretmez.
    private async Task EnsureGlobalAsync(string name, string defaultValue)
    {
        var current = await _settingManager.GetOrNullGlobalAsync(name, fallback: false);
        if (string.IsNullOrEmpty(current))
        {
            await _settingManager.SetAsync(
                name,
                defaultValue,
                GlobalSettingValueProvider.ProviderName,
                providerKey: null,
                forceToSet: true);
        }
    }
}
