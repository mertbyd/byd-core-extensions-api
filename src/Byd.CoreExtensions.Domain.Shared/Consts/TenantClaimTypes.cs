namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Tenant kapsaminin token uzerinde tasindigi claim adlari.
/// </summary>
// islevi: Kapsami token'a yazan taraf ile istekte okuyan tarafin ayni claim adini kullanmasini saglar.
// sistemdeki gorevi: Secilen tenant'i tasir; cercevenin tenant claim'i kullanicinin kendi tenant'ina aittir.
public static class TenantClaimTypes
{
    // Istegin hangi tenant adina calistigini bildiren claim.
    public const string TenantId = "tenant_id";

    // Tenant'in okunabilir adi; istemci gostermek icin ayrica sorgu atmaz.
    public const string TenantName = "tenant_name";
}
