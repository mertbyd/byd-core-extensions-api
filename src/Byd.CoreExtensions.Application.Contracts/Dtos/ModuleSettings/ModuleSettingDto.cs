using System;

namespace Byd.CoreExtensions.Dtos.ModuleSettings;

/// <summary>
/// Kalici modul ayari kaydinin public cevap alanlarini tasir.
/// </summary>
// islevi: ModuleSetting entity alanlarini ve son guncelleme zamanini HTTP ciktisina acar.
// sistemdeki gorevi: Entity veya EF tiplerini disariya sizdirmayan response sozlesmesidir.
public class ModuleSettingDto
{
    public Guid Id { get; set; }
    public string ModuleName { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public bool IsEnabled { get; set; }
    public int TtlSeconds { get; set; }
    public bool RateLimitEnabled { get; set; }
    public int RateLimitPermitLimit { get; set; }
    public int RateLimitWindowSeconds { get; set; }
    public bool IdempotencyEnabled { get; set; }
    public int IdempotencyTtlSeconds { get; set; }
    public bool EventBoxEnabled { get; set; }
    public bool FailedEventRetryUnlimited { get; set; }
    public int FailedEventRetryWindowSeconds { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
