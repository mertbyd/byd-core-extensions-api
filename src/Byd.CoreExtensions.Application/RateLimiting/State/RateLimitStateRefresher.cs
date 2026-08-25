using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit ayarlarini asenkron okuyup bellek durumunu tazeler.
/// </summary>
// islevi: Uc dinamik ayari ISettingProvider ile okur ve RateLimitState.Update ile yeni versiyonlu snapshot'a yazar.
// sistemdeki gorevi: Ayar okuma isini limiter policy'nin disinda tutar; cagrildiginda hazir snapshot'i atomik olarak yeniler.
public class RateLimitStateRefresher : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;
    private readonly RateLimitState _state;

    public RateLimitStateRefresher(ISettingProvider settingProvider, RateLimitState state)
    {
        _settingProvider = settingProvider;
        _state = state;
    }

    // Ayarlari asenkron okur ve snapshot'i yeni versiyonla tazeler; okunamayan deger sabit varsayilana duser.
    public async Task RefreshAsync()
    {
        var algorithm = await _settingProvider.GetOrNullAsync(CoreExtensionsSettings.RateLimitDefaultAlgorithm);
        var permitLimit = await _settingProvider.GetAsync<int>(
            CoreExtensionsSettings.RateLimitDefaultPermitLimit,
            RateLimitConsts.DefaultPermitLimit);
        var windowSeconds = await _settingProvider.GetAsync<int>(
            CoreExtensionsSettings.RateLimitDefaultWindowSeconds,
            RateLimitConsts.DefaultWindowSeconds);
        _state.Update(algorithm ?? RateLimitConsts.DefaultAlgorithm, permitLimit, windowSeconds);
    }
}
