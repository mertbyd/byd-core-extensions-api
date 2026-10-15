using System;
using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.Attributes;

/// <summary>
/// Bir action'in yanitinin Redis'te saklanip tekrar eden isteklerde oradan donulecegini bildiren oznitelik.
/// </summary>
// islevi: Yanitin bagli oldugu veri grubunu, onbellekte kalma suresini ve kullaniciya ozel olup olmadigini tasir.
// sistemdeki gorevi: Yanit onbellegi filtresinin hangi action'lari saklayacagini okudugu tek kaynak. Anahtar tenant,
// organizasyon, kultur, yol, sorgu ve govdeden kurulur; grup temizlenince o gruptaki tum yanitlar duser.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CachedResponseAttribute : Attribute
{
    // Grubu zorunlu alir; grubu olmayan yanit hic temizlenemez, yalniz suresi dolunca duser.
    public CachedResponseAttribute(string group)
    {
        Group = group;
    }

    // Yanitin bagli oldugu veri grubu; cogunlukla yanitin kaynagi olan entity'nin adidir.
    public string Group { get; }

    // Yanitin onbellekte kalma suresi (dakika).
    public int DurationMinutes { get; set; } = CachedResponseConsts.DefaultDurationMinutes;

    // Yanit istegi yapan kullaniciya gore degisiyorsa anahtara kullanici kimligi eklenir.
    public bool IsUserScoped { get; set; }
}
