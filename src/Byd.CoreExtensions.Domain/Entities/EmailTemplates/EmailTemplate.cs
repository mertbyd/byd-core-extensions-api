using System;
using Byd.CoreExtensions.Entities.EmailEvents;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.CoreExtensions.Entities.EmailTemplates;

/// <summary>
/// Bir e-posta olayinin belirli kapsam ve dildeki konu ve icerik sablonunu tasir.
/// </summary>
// islevi: Olay, dil, kapsam, aktiflik ve sablon metinlerini audited aggregate olarak kalici tutar.
// sistemdeki gorevi: Is kurallarini EmailTemplateManager'a birakan veri kabugudur. Ayni olay ve dil icin her kapsamda
// tek aktif satir bulunur; gonderim organizasyon, ust organizasyon, tenant ve host satirlari arasindan en yakin aktif
// satiri secer.
public class EmailTemplate : AuditedAggregateRoot<Guid>
{
    // Satirin tenant kapsami; null ise host satiridir ve alt kapsamlarda aktif satir yoksa kullanilir.
    // Bilerek IMultiTenant degildir: ABP'nin global filtresi tenant istegi sirasinda host satirini gizler,
    // gonderim zinciri ise tenant satiri yoksa host satirina duser.
    public Guid? TenantId { get; internal set; }

    // Satirin organizasyon kapsami; null ise tenant geneli satirdir. Ayni sebeple organizasyon filtresine baglanmaz.
    public Guid? OrganizationId { get; internal set; }

    // Sablonun bagli oldugu olay kaydinin kimligi.
    public Guid EmailEventId { get; internal set; }

    // DTO'daki EmailEventId-EmailEventName ciftinin guncel ad kaynagidir.
    public EmailEvent EmailEvent { get; internal set; } = default!;
    public string Name { get; internal set; } = default!;
    public string CultureName { get; internal set; } = default!;
    public string Subject { get; internal set; } = default!;
    public bool IsActive { get; internal set; }
    public string Body { get; internal set; } = default!;

    // EF Core materializasyonu icin parametresiz ctor.
    protected EmailTemplate()
    {
    }

    // Manager tarafindan uretilen kimlikle bos veri kabugunu kurar.
    internal EmailTemplate(Guid id)
        : base(id)
    {
    }
}
