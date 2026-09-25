using System;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Entity'nin organizasyon kapsamina bagli oldugunu bildiren sozlesme.
/// </summary>
// islevi: Tipin OrganizationId alanina sahip oldugunu bildirir; global sorgu filtresi ve yazma damgasi bu isarete bakar.
// sistemdeki gorevi: IMultiTenant'in organizasyon karsiligi. Entity iki kapsami ayri ayri beyan eder; organizasyona
// bagli olmak tenant'a bagli olmayi zorunlu kilmaz.
public interface IMultiOrganization
{
    // Kaydin ait oldugu organizasyon. Deger yazma aninda sunucudan damgalanir; istekten alinmaz, cunku istekten
    // gelen bir kapsam yanlis yazildiginda hicbir sorgunun bulamayacagi bir satir birakir ve hata vermez.
    Guid? OrganizationId { get; }
}
