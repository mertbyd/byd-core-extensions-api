using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Providers;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit ayarlarini asenkron okuyup bellek durumunu tazeler.
/// </summary>
// islevi: Bu modulun host kapsamli tablo satirini repository ile okur ve RateLimitState.Update ile yeni versiyonlu snapshot'a yazar.
// sistemdeki gorevi: ModuleSetting okumasini limiter policy'nin disinda tutar; snapshot ile sonraki yenileme TTL'ini birlikte tazeler.
// Kapsam bilerek host'tur: rate limit middleware'i tenant baglamini kuran resource filter'dan once calisir, dolayisiyla limiter tenant satirini goremez.
public class RateLimitStateRefresher : ITransientDependency
{
    private readonly IModuleSettingRepository _repository;
    private readonly CurrentModuleNameProvider _moduleNameProvider;
    private readonly RateLimitState _state;
    public RateLimitStateRefresher(
        IModuleSettingRepository repository,
        CurrentModuleNameProvider moduleNameProvider,
        RateLimitState state)
    {
        _repository = repository;
        _moduleNameProvider = moduleNameProvider;
        _state = state;
    }
    // Son okunan satirin worker tarafindan kullanilacak yenileme suresini tasir.
    public int LastTtlSeconds { get; private set; } = ModuleSettingConsts.DefaultTtlSeconds;

    // Modul satirini asenkron okur; satir yoksa rate limit'i kapali sabit varsayilanlara dondurur.
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var moduleSetting = await _repository.FindForScopeAsync(
            _moduleNameProvider.Value,
            tenantId: null,
            cancellationToken);
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
