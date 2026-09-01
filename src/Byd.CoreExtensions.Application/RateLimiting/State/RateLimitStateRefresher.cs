using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ModuleSettings;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit ayarlarini asenkron okuyup bellek durumunu tazeler.
/// </summary>
// islevi: Bu modulun host kapsamli tablo satirini merkezi ayar servisiyle okur ve RateLimitState.Update ile yeni versiyonlu snapshot'a yazar.
// sistemdeki gorevi: ModuleSetting okumasini limiter policy'nin disinda tutar; snapshot ile sonraki yenileme TTL'ini birlikte tazeler.
// Kapsam bilerek host'tur: rate limit middleware'i tenant baglamini kuran resource filter'dan once calisir, dolayisiyla limiter tenant satirini goremez.
public class RateLimitStateRefresher : ITransientDependency
{
    private readonly IModuleSettingProvider _provider;
    private readonly RateLimitState _state;

    public RateLimitStateRefresher(
        IModuleSettingProvider provider,
        RateLimitState state)
    {
        _provider = provider;
        _state = state;
    }

    // Son okunan satirin worker tarafindan kullanilacak yenileme suresini tasir.
    public int LastTtlSeconds { get; private set; } = ModuleSettingConsts.DefaultTtlSeconds;

    // Modul satirini asenkron okur; satir yoksa rate limit'i kapali sabit varsayilanlara dondurur.
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _provider.RefreshHostAsync(cancellationToken);
        if (setting is null)
        {
            LastTtlSeconds = ModuleSettingConsts.DefaultTtlSeconds;
            _state.Update(
                isEnabled: false,
                RateLimitConsts.DefaultPermitLimit,
                RateLimitConsts.DefaultWindowSeconds);
            return;
        }

        LastTtlSeconds = setting.TtlSeconds;
        // Modul ana anahtari kapaliyken alt anahtar tek basina yetmez; rate limit de kapanir.
        // Mukerrer istek korumasi da ayni kurali uygular, iki koruma tek ana anahtardan beslenir.
        _state.Update(
            setting.IsEnabled && setting.RateLimitEnabled,
            setting.RateLimitPermitLimit,
            setting.RateLimitWindowSeconds);
    }
}
