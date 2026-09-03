namespace Byd.CoreExtensions.ModuleSettings;

/// <summary>
/// Modul ayarinin cache'te tutulan merkezi goruntusunu tasir.
/// </summary>
// islevi: ModuleSetting satirinin istek ve isci yollarinda gereken alanlarini sade bir modele projekte eder.
// sistemdeki gorevi: Ayar tablosu tuketicilerine repository ve entity bagimliligi olmadan ortak, serilestirilebilir veri saglar.
public sealed class ModuleSettingCacheModel
{
    public bool IsEnabled { get; set; }
    public bool IdempotencyEnabled { get; set; }
    public int IdempotencyTtlSeconds { get; set; }
    public bool EventBoxEnabled { get; set; }
    public bool FailedEventRetryUnlimited { get; set; }
    public int FailedEventRetryWindowSeconds { get; set; }
    public bool RateLimitEnabled { get; set; }
    public int RateLimitPermitLimit { get; set; }
    public int RateLimitWindowSeconds { get; set; }
    public int TtlSeconds { get; set; }
}
