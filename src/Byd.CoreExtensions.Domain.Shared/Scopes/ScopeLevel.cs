namespace Byd.CoreExtensions.Scopes;

/// <summary>
/// Bir kullanici oturumunun veya rolun ait olabilecegi kapsam seviyeleri.
/// </summary>
// islevi: Super admin, tenant ve organizasyon kapsamlarini mevcut veriden turetilen derleme guvenli seviyeler olarak tanimlar.
// sistemdeki gorevi: Moduller ve sunucunun urettigi kapsam claim'i arasinda ortak seviye sozlesmesini saglar.
public enum ScopeLevel
{
    Unselected = 0,
    SuperAdmin = 1,
    Tenant = 2,
    Organization = 3,
    SubOrganization = 4
}
