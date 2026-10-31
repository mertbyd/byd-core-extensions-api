using System;

namespace Byd.CoreExtensions.Models.Organizations;

/// <summary>
/// Yururlukteki organizasyon kapsaminin degerleri.
/// </summary>
// islevi: Kapsamin kimligini, adini ve alt organizasyonsa ust organizasyonunu tek deger olarak tasir.
// sistemdeki gorevi: Kapsam degistiginde kimlik, ad ve ust organizasyon birlikte degisir; ayri tutmak, kapsam
// degistikten sonra onceki organizasyonun degerlerinin kalmasina izin verirdi. Kimlik ve ad tenant tarafindaki
// karsiligiyla ayni adlari tasir; ust organizasyon alanlari yalniz bu tarafta vardir cunku tenant'lar hiyerarsi
// kurmaz.
public class BasicOrganizationInfo(
    Guid? organizationId,
    string? name = null,
    Guid? parentId = null,
    string? parentName = null)
{
    // Kapsamdaki organizasyon; null ise kapsam acilmamis demektir.
    public Guid? OrganizationId { get; } = organizationId;

    // OrganizationId doluysa organizasyonun adi.
    public string? Name { get; } = name;

    // Kapsamdaki organizasyon alt organizasyonsa ust organizasyonun kimligi.
    public Guid? ParentId { get; } = parentId;

    // Kapsamdaki organizasyon alt organizasyonsa ust organizasyonun adi.
    public string? ParentName { get; } = parentName;
}
