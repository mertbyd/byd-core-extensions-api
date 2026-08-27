using System;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Bellege alinmis tek bir API anahtarinin dogrulama icin gereken salt-okunur goruntusu.
/// </summary>
// islevi: Anahtarin kimligini, adini, son kullanma zamanini ve sahiplik alanlarini degismez sekilde tasir.
// sistemdeki gorevi: EF entity'sinden ayrik, DbContext yasam dongusune bagli olmayan; store'da tutulan ve basarili dogrulamada request context'e aktarilan tasiyici.
public sealed class ApiKeyDescriptor
{
    public ApiKeyDescriptor(
        Guid id,
        string name,
        DateTime? expiresAt,
        Guid? tenantId,
        Guid? organizationId)
    {
        Id = id;
        Name = name;
        ExpiresAt = expiresAt;
        TenantId = tenantId;
        OrganizationId = organizationId;
    }
    // Anahtarin benzersiz kimligi.
    public Guid Id { get; }

    // Anahtarin insan tarafindan okunabilir adi.
    public string Name { get; }

    // Son gecerlilik zamani (UTC); null ise suresizdir.
    public DateTime? ExpiresAt { get; }

    // Ait oldugu tenant kimligi.
    public Guid? TenantId { get; }

    // Bagli oldugu organizasyon kimligi.
    public Guid? OrganizationId { get; }
}
