using System;

namespace Byd.CoreExtensions.Dtos.ModuleSettings;

/// <summary>
/// Modul ayarinin rate limit okuma cevap alanlarini tasir.
/// </summary>
// islevi: ModuleSetting kaydinin ortak anahtarlarini ve rate limit alanlarini HTTP ciktisina acar.
// sistemdeki gorevi: Idempotency alanlarini sizdirmeden rate limit ekraninin okuma sozlesmesini tamamlar.
public class ModuleSettingRateLimitDto
{
    public Guid Id { get; set; }
    public string ModuleName { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public bool IsEnabled { get; set; }
    public int TtlSeconds { get; set; }
    public bool RateLimitEnabled { get; set; }
    public int RateLimitPermitLimit { get; set; }
    public int RateLimitWindowSeconds { get; set; }
}
