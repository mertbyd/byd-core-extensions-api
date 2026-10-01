using System;

namespace Byd.CoreExtensions.Dtos.ApiKeys;

/// <summary>
/// Kalici API anahtari kaydinin public cevap alanlarini tasir.
/// </summary>
// islevi: ApiKey kimligini, adini, son kullanma zamanini, sahiplik alanlarini ve olusturulma zamanini HTTP ciktisina acar.
// sistemdeki gorevi: Entity veya EF tiplerini disariya sizdirmayan response sozlesmesi; anahtar degeri bilerek yer almaz, yalnizca uretme cevabinda doner.
public class ApiKeyDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OrganizationId { get; set; }
    public string Name { get; set; } = default!;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreationTime { get; set; }
}
