using System;

namespace Byd.CoreExtensions.Dtos.ModuleSettings;

/// <summary>
/// Modul ayarinin idempotency okuma cevap alanlarini tasir.
/// </summary>
// islevi: ModuleSetting kaydinin ortak anahtarlarini ve idempotency alanlarini HTTP ciktisina acar.
// sistemdeki gorevi: Rate limit alanlarini sizdirmeden idempotency ekraninin okuma sozlesmesini tamamlar.
public class ModuleSettingIdempotencyDto
{
    public Guid Id { get; set; }
    public string ModuleName { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IdempotencyEnabled { get; set; }
    public int IdempotencyTtlSeconds { get; set; }
}
