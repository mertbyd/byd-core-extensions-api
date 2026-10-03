namespace Byd.CoreExtensions.Models.EmailEvents;

/// <summary>
/// Yeni e-posta olayi kaydinin domain create girdisini tanimlar.
/// </summary>
// islevi: Olayi seed eden modulun verdigi ad ve kodu Manager'in entity kurma akimina tasir.
// sistemdeki gorevi: EmailEvent olusturma senaryosunun transporttan bagimsiz girdi tipidir.
public sealed class EmailEventCreateModel
{
    public string Name { get; set; } = default!;
    public int? Code { get; set; }
}
