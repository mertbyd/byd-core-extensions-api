using Byd.CoreExtensions.Models.Organizations;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Acik olarak degistirilen organizasyon kapsamini tutan saklama noktasi.
/// </summary>
// islevi: Change ile acilan kapsami cagri zinciri boyunca tasir; acik bir degisiklik yoksa null kalir.
// sistemdeki gorevi: ICurrentOrganization'in once baktigi ustelenmis kapsamin kaynagi. Ayri bir arayuz oldugu icin
// kapsami okuyan istek kapsamli tip ile kapsami tutan tekil tip birbirinden bagimsiz kalir.
public interface ICurrentOrganizationAccessor
{
    // Yururlukteki ustelenmis kapsam; yoksa null.
    BasicOrganizationInfo? Current { get; set; }
}
