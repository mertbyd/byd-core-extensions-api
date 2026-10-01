using System;

namespace Byd.CoreExtensions.Models.Organizations;

/// <summary>
/// Yururlukteki organizasyon kapsaminin degerleri.
/// </summary>
// islevi: Kapsamin kimligini ve adini tek deger olarak tasir.
// sistemdeki gorevi: Kapsam degistiginde kimlik ve ad birlikte degisir; ikisini ayri tutmak, kapsam degistikten
// sonra adin onceki organizasyondan kalmasina izin verirdi. Ad ve uye adlari tenant tarafindaki karsiligiyla
// birebir ayni tutulur ki iki kapsam yan yana okundugunda aradaki fark yalnizca alanin adi olsun.
public class BasicOrganizationInfo(Guid? organizationId, string? name = null)
{
    // Kapsamdaki organizasyon; null ise kapsam acilmamis demektir.
    public Guid? OrganizationId { get; } = organizationId;

    // OrganizationId doluysa organizasyonun adi.
    public string? Name { get; } = name;
}
