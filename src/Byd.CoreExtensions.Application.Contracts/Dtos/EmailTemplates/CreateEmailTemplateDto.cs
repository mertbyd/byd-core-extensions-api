using System;

namespace Byd.CoreExtensions.Dtos.EmailTemplates;

/// <summary>
/// Yeni e-posta sablonu olusturma istegini tanimlar.
/// </summary>
// islevi: EmailTemplate create endpointinin public girdi tipidir; olay ve dil yalniz burada secilir.
// sistemdeki gorevi: Kimlik, kapsam ve aktiflik alanlarini sunucuda birakip degistirilebilir alanlari kabul eder.
public sealed class CreateEmailTemplateDto : EmailTemplateInputDto
{
    public Guid EmailEventId { get; set; }
    public string CultureName { get; set; } = default!;
}
