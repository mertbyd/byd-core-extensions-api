namespace Byd.CoreExtensions.Models.EmailTemplates;

/// <summary>
/// E-posta sablonu create ve update islemlerinin ortak domain girdisini tasir.
/// </summary>
// islevi: Public DTO alanlarini entity mutasyonundan ayiran tipli domain modelini tanimlar.
// sistemdeki gorevi: Mapperly ile EmailTemplateManager arasindaki ortak girdi sozlesmesidir.
public abstract class EmailTemplateInputModel
{
    public string Name { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
}
