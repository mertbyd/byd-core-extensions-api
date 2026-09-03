using System;
using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.Models.ModuleSettings;

/// <summary>
/// Modul ayari create ve update islemlerinin ortak domain girdisini tasir.
/// </summary>
// islevi: Public DTO alanlarini entity mutasyonundan ayiran tipli domain modelini tanimlar.
// sistemdeki gorevi: Mapperly ile ModuleSettingManager arasindaki ortak girdi sozlesmesidir.
public abstract class ModuleSettingInputModel
{
    public string ModuleName { get; set; } = default!;

    // Satirin kapsami; null birakilirsa host satiri olarak yazilir ve tum tenant'lar icin gecerli olur.
    public Guid? TenantId { get; set; }

    public bool IsEnabled { get; set; }
    public int TtlSeconds { get; set; } = ModuleSettingConsts.DefaultTtlSeconds;
    public bool RateLimitEnabled { get; set; }
    public int RateLimitPermitLimit { get; set; } = ModuleSettingConsts.DefaultRateLimitPermitLimit;
    public int RateLimitWindowSeconds { get; set; } = ModuleSettingConsts.DefaultRateLimitWindowSeconds;
    public bool IdempotencyEnabled { get; set; }
    public int IdempotencyTtlSeconds { get; set; } = IdempotencyConsts.DefaultTtlSeconds;
    public bool EventBoxEnabled { get; set; } = EventBoxConsts.DefaultEnabled;
    public bool FailedEventRetryUnlimited { get; set; } = EventBoxConsts.DefaultFailedEventRetryUnlimited;
    public int FailedEventRetryWindowSeconds { get; set; }
        = EventBoxConsts.DefaultFailedEventRetryWindowSeconds;
}
