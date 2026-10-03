using System;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.CoreExtensions.Entities.EmailEvents;

/// <summary>
/// E-posta sablonlarinin bagli oldugu olay kaydini tasir.
/// </summary>
// islevi: Olayin adini ve istege bagli is kodunu kalici olarak tutar.
// sistemdeki gorevi: Kurallari EmailEventManager'a birakan ince lookup persistence kabugudur. Sablon satiri olaya bu
// kaydin kimligiyle baglanir; olayi gonderen modul satiri kendi seed'iyle acar ve gonderimde adiyla bulur.
public class EmailEvent : CreationAuditedEntity<Guid>, ILookup
{
    // Olayin tekil adi; olayi gonderen modulun sabitinden gelir.
    public string Name { get; internal set; } = default!;

    // Is tarafinin istege bagli olay kodudur.
    public int? Code { get; internal set; }

    // EF Core materializasyonu icin bos kurucudur.
    protected EmailEvent()
    {
    }

    // Manager tarafindan uretilen kimlikle bos veri kabugunu kurar.
    internal EmailEvent(Guid id)
        : base(id)
    {
    }

    // ILookup yazma yuzeyini Domain disina public entity mutasyonu acmadan uygular.
    string ILookup.Name
    {
        get => Name;
        set => Name = value;
    }

    // ILookup kod yazma yuzeyini Domain disina public entity mutasyonu acmadan uygular.
    int? ILookup.Code
    {
        get => Code;
        set => Code = value;
    }
}
