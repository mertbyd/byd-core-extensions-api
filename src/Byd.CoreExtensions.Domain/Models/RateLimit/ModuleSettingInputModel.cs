using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.Models.RateLimit;

/// <summary>
/// Modul ayari create ve update islemlerinin ortak domain girdisini tasir.
/// </summary>
// islevi: Public DTO alanlarini entity mutasyonundan ayiran tipli domain modelini tanimlar.
// sistemdeki gorevi: Mapperly ile ModuleSettingManager arasindaki ortak girdi sozlesmesidir.
public abstract class ModuleSettingInputModel
{
    public string ModuleName { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public int TtlSeconds { get; set; } = ModuleSettingConsts.DefaultTtlSeconds;
    public bool RateLimitEnabled { get; set; }
    public int RateLimitPermitLimit { get; set; } = ModuleSettingConsts.DefaultRateLimitPermitLimit;
    public int RateLimitWindowSeconds { get; set; } = ModuleSettingConsts.DefaultRateLimitWindowSeconds;
}
