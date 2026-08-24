using System;

namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Filtre modeli property'sini entity alanina ve hedef tipe baglayan oznitelik.
/// </summary>
// islevi: Bir filtre alaninin disariya acilan adini, entity uzerindeki property yolunu ve gerekiyorsa donusturulecek hedef tipi bildirir.
// sistemdeki gorevi: Izin listesinin kaynagi; bu oznitelige sahip olmayan alan filtrelenemez, boylece property adi hicbir zaman dogrudan istemciden gelmez.
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class DataTableAttribute : Attribute
{
    public DataTableAttribute(string name, string propertyPath, Type? targetType = null)
    {
        Name = name;
        PropertyPath = propertyPath;
        TargetType = targetType;
    }

    // Filtrenin disariya acilan adi; istemcinin gordugu ve gonderdigi isim budur.
    public string Name { get; }

    // Entity uzerindeki property yolu; noktali yazimla iliskili nesneye inilebilir.
    public string PropertyPath { get; }

    // Ham metnin cevrilecegi hedef tip; null birakilirsa motor entity property tipinden cikarir.
    public Type? TargetType { get; }
}
