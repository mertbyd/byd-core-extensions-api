using System;

namespace Byd.CoreExtensions.Dtos.ModuleSettings;

/// <summary>
/// Modul ayarinin olay kutusu okuma cevap alanlarini tasir.
/// </summary>
// islevi: ModuleSetting kaydinin ortak anahtarlarini ve olay kutusu alanlarini HTTP ciktisina acar.
// sistemdeki gorevi: Rate limit ve idempotency alanlarini sizdirmeden olay kutusu ekraninin okuma sozlesmesini tamamlar.
public class ModuleSettingEventBoxDto
{
    public Guid Id { get; set; }
    public string ModuleName { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public bool IsEnabled { get; set; }
    public bool EventBoxEnabled { get; set; }
    public bool FailedEventRetryUnlimited { get; set; }
    public int FailedEventRetryWindowSeconds { get; set; }
}
