namespace Byd.CoreExtensions.Dtos.EmailTemplates;

/// <summary>
/// E-posta sablonu create ve update isteklerinin ortak alanlarini tasir.
/// </summary>
// islevi: Public CRUD girdisinin degistirilebilir ad, konu ve icerik alanlarini tanimlar.
// sistemdeki gorevi: FluentValidation ile Mapperly arasindaki ortak request sozlesmesidir.
public abstract class EmailTemplateInputDto
{
    public string Name { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
}
