using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.Dtos.RateLimit;

/// <summary>
/// Modul ayari create ve update isteklerinin ortak alanlarini tasir.
/// </summary>
// islevi: Public CRUD girdisinin degistirilebilir modul ve rate limit alanlarini tanimlar.
// sistemdeki gorevi: FluentValidation ile Mapperly arasindaki ortak request sozlesmesidir.
public abstract class ModuleSettingInputDto
{
    public string ModuleName { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public int TtlSeconds { get; set; } = ModuleSettingConsts.DefaultTtlSeconds;
    public bool RateLimitEnabled { get; set; }
    public int RateLimitPermitLimit { get; set; } = ModuleSettingConsts.DefaultRateLimitPermitLimit;
    public int RateLimitWindowSeconds { get; set; } = ModuleSettingConsts.DefaultRateLimitWindowSeconds;
}
