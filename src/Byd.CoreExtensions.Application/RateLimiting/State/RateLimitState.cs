using Byd.CoreExtensions.Consts;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit global varsayilanlarinin senkron okunabilen, versiyonlu bellek durumu.
/// </summary>
// islevi: Guncel snapshot'i tutar; ayar degistiginde yeni versiyonlu snapshot ile atomik olarak degistirir.
// sistemdeki gorevi: Dinamik rate limit'in kalbi; limiter factory bu durumu kilitlemeden okur, ayar okuyucu ise Update ile tazeler.
public class RateLimitState : IRateLimitState, ISingletonDependency
{
    private volatile RateLimitSnapshot _current = new(
        isEnabled: false,
        RateLimitConsts.DefaultAlgorithm,
        RateLimitConsts.DefaultPermitLimit,
        RateLimitConsts.DefaultWindowSeconds,
        version: 0);
    // Limiter factory'nin her istekte senkron okudugu guncel snapshot.
    public RateLimitSnapshot Current => _current;

    // Yeni ayar degerlerini bir sonraki versiyonla snapshot'a yazar; okuma tarafi kilitlenmez.
    public void Update(bool isEnabled, int permitLimit, int windowSeconds)
    {
        _current = new RateLimitSnapshot(
            isEnabled,
            RateLimitConsts.DefaultAlgorithm,
            permitLimit,
            windowSeconds,
            _current.Version + 1);
    }
}
