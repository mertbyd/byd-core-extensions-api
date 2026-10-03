using System;

namespace Byd.CoreExtensions.Models.EmailTemplates;

/// <summary>
/// Yeni e-posta sablonu kaydinin domain create girdisini tanimlar.
/// </summary>
// islevi: Create DTO eslemesini Manager'in entity kurma akimina tasir; olay ve dil yalniz olusturmada secilir.
// sistemdeki gorevi: EmailTemplate olusturma senaryosunun transporttan bagimsiz girdi tipidir.
public sealed class EmailTemplateCreateModel : EmailTemplateInputModel
{
    public Guid EmailEventId { get; set; }
    public string CultureName { get; set; } = default!;
}
