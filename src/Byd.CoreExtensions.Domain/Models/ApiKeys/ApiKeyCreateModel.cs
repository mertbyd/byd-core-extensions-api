using System;

namespace Byd.CoreExtensions.Models.ApiKeys;

/// <summary>
/// Yeni API anahtari kaydinin domain create girdisini tanimlar.
/// </summary>
// islevi: Public DTO alanlarini entity mutasyonundan ayiran tipli domain modelini tanimlar.
// sistemdeki gorevi: Mapperly ile ApiKeyManager arasindaki transporttan bagimsiz girdi sozlesmesidir.
public sealed class ApiKeyCreateModel
{
    public string Name { get; set; } = default!;
    public DateTime? ExpiresAt { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OrganizationId { get; set; }
}
