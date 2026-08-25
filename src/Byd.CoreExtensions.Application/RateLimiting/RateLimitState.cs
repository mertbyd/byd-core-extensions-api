using Byd.CoreExtensions.Consts;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit global varsayilanlarinin senkron okunabilen, versiyonlu bellek durumu.
/// </summary>
// islevi: Guncel snapshot'i tutar; ayar degistiginde yeni versiyonlu snapshot ile atomik olarak degistirir.
// sistemdeki gorevi: Dinamik rate limit'in kalbi; limiter factory bu durumu kilitlemeden okur, ayar okuyucu ise Update ile tazeler.
public class RateLimitState : ISingletonDependency
{
    private volatile RateLimitSnapshot _current = new(
        RateLimitConsts.DefaultAlgorithm,
        RateLimitConsts.DefaultPermitLimit,
        RateLimitConsts.DefaultWindowSeconds,
        version: 0);

    // Limiter factory'nin her istekte senkron okudugu guncel snapshot.
    public RateLimitSnapshot Current => _current;

    // Yeni ayar degerlerini bir sonraki versiyonla snapshot'a yazar; okuma tarafi kilitlenmez.
    public void Update(string algorithm, int permitLimit, int windowSeconds)
    {
        _current = new RateLimitSnapshot(algorithm, permitLimit, windowSeconds, _current.Version + 1);
    }
}
