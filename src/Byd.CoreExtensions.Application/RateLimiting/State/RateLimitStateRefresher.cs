using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit ayarlarini asenkron okuyup bellek durumunu tazeler.
/// </summary>
// islevi: Bu modulun tablo satirini repository ile okur ve RateLimitState.Update ile yeni versiyonlu snapshot'a yazar.
// sistemdeki gorevi: ModuleSetting okumasini limiter policy'nin disinda tutar; snapshot ile sonraki yenileme TTL'ini birlikte tazeler.
public class RateLimitStateRefresher : ITransientDependency
{
    private readonly IModuleSettingRepository _repository;
    private readonly RateLimitState _state;
    public RateLimitStateRefresher(
        IModuleSettingRepository repository,
        RateLimitState state)
    {
        _repository = repository;
        _state = state;
    }
    // Son okunan satirin worker tarafindan kullanilacak yenileme suresini tasir.
    public int LastTtlSeconds { get; private set; } = ModuleSettingConsts.DefaultTtlSeconds;

    // Modul satirini asenkron okur; satir yoksa rate limit'i kapali sabit varsayilanlara dondurur.
    public async Task RefreshAsync()
    {
        var moduleSetting = await _repository.FindAsync(
            setting => setting.ModuleName == ModuleSettingConsts.CurrentModuleName);
        if (moduleSetting is null)
        {
            LastTtlSeconds = ModuleSettingConsts.DefaultTtlSeconds;
            _state.Update(
                isEnabled: false,
                RateLimitConsts.DefaultPermitLimit,
                RateLimitConsts.DefaultWindowSeconds);
            return;
        }
        LastTtlSeconds = moduleSetting.TtlSeconds;
        _state.Update(
            moduleSetting.RateLimitEnabled,
            moduleSetting.RateLimitPermitLimit,
            moduleSetting.RateLimitWindowSeconds);
    }
}
