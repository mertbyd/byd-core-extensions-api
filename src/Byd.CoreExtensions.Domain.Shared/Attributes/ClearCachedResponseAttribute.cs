using System;

namespace Byd.CoreExtensions.Attributes;

/// <summary>
/// Bir servis metodu basariyla tamamlaninca verilen yanit grubunun onbellekten silinecegini bildiren oznitelik.
/// </summary>
// islevi: Ekleme, guncelleme ya da silme metodunun degistirdigi veri grubunu tasir.
// sistemdeki gorevi: [CachedResponse] ile saklanan okumalarin ayni grubu degistiren yazmadan sonra dusmesini saglar. Temizlik
// islem commit edilince yapilir; hata veren ya da geri alinan yazma onbellegi silmez.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ClearCachedResponseAttribute : Attribute
{
    // Grubu zorunlu alir; okuma metotlarindaki [CachedResponse] grubuyla ayni ad verilir.
    public ClearCachedResponseAttribute(string group)
    {
        Group = group;
    }

    // Yazmadan sonra temizlenecek veri grubu.
    public string Group { get; }
}
