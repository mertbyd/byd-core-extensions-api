namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit motorunun guncel varsayilan snapshot'ini sunar.
/// </summary>
// islevi: HttpApi policy'sinin Application implementasyonuna baglanmadan guncel snapshot'i okumasini saglar.
// sistemdeki gorevi: Application state yonetimi ile HTTP rate limit motoru arasindaki tek yonlu contract siniridir.
public interface IRateLimitState
{
    RateLimitSnapshot Current { get; }
}
