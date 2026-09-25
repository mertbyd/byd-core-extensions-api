using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.CoreExtensions.Entities;

/// <summary>
/// Bir istemciye verilen API anahtarini ve dogrulama icin gereken bilgiyi tasiyan kayit.
/// </summary>
// islevi: Anahtar degerini, adini, son kullanma bilgisini ve sahiplik (tenant/organizasyon) alanlarini kalici olarak tutar.
// sistemdeki gorevi: API anahtari dogrulamasinin tek veri kaynagi. Sahiplik alanlari yalnizca tasinan veridir; global sorgu filtresine baglanmaz, cunku anahtar istege kapsam bilgisi olmadan gelir ve kapsami zaten kendisi belirler.
public class ApiKey : CreationAuditedEntity<Guid>
{
    // Anahtarin ait oldugu tenant; istek bu anahtarla geldiginde tenant baglami buradan kurulur.
    public Guid? TenantId { get; internal set; }

    // Anahtarin bagli oldugu organizasyon; istek baglamina tasinir, downstream kod anahtardan okur.
    public Guid? OrganizationId { get; internal set; }

    // Istemcinin gonderdigi anahtar degeri; istekteki header degeriyle karsilastirilir.
    public string Key { get; internal set; } = default!;

    // Anahtarin insan tarafindan okunabilir adi/etiketi.
    public string Name { get; internal set; } = default!;

    // Anahtarin son gecerlilik zamani (UTC); null ise anahtarin suresi hic dolmaz.
    public DateTime? ExpiresAt { get; internal set; }

    protected ApiKey()
    {
    }

    // Uretilmis anahtar degeriyle yeni kayit kurar; kalan alanlari Manager atar.
    internal ApiKey(Guid id, string key)
        : base(id)
    {
        Key = key;
    }
}
