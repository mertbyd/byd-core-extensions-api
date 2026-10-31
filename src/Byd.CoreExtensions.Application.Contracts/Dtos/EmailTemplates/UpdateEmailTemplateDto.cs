namespace Byd.CoreExtensions.Dtos.EmailTemplates;

/// <summary>
/// Mevcut e-posta sablonunu guncelleme istegini tanimlar.
/// </summary>
// islevi: EmailTemplate update endpointinin public girdi tipidir.
// sistemdeki gorevi: Olay, dil ve kapsami sabit tutup yalniz ortak sablon alanlarini kabul eder.
public sealed class UpdateEmailTemplateDto : EmailTemplateInputDto
{
}
