using System;

namespace Byd.CoreExtensions.Dtos.EmailTemplates;

/// <summary>
/// Kalici e-posta sablonu kaydinin public cevap alanlarini tasir.
/// </summary>
// islevi: EmailTemplate olay, dil, aktiflik ve sablon metinlerini HTTP ciktisina acar.
// sistemdeki gorevi: Entity veya EF tiplerini disariya sizdirmayan response sozlesmesidir. Kapsam alanlari yer almaz;
// her uc yalniz istegin kendi kapsamindaki satirlari dondurur.
public class EmailTemplateDto
{
    public Guid Id { get; set; }
    public Guid EmailEventId { get; set; }
    public string EmailEventName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string CultureName { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public string Body { get; set; } = default!;
}
