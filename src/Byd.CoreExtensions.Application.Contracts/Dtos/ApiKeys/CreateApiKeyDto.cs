using System;

namespace Byd.CoreExtensions.Dtos.ApiKeys;

/// <summary>
/// Yeni API anahtari uretme istegini tanimlar.
/// </summary>
// islevi: ApiKey uretim endpointinin public girdi tipidir; ad, istege bagli son kullanma zamani ve sahiplik alanlarini tasir.
// sistemdeki gorevi: Anahtar degerini bilerek disarida birakir; anahtari sunucu uretir, sahiplik alanlarini cagiran belirler.
public sealed class CreateApiKeyDto
{
    public string Name { get; set; } = default!;
    public DateTime? ExpiresAt { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OrganizationId { get; set; }
}
