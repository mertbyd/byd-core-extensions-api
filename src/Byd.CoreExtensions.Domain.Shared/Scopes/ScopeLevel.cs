namespace Byd.CoreExtensions.Scopes;

/// <summary>
/// Bir kullanici oturumunun veya rolun ait olabilecegi kapsam seviyeleri.
/// </summary>
// islevi: Super admin, tenant ve organizasyon kapsamlarini mevcut veriden turetilen derleme guvenli seviyeler olarak tanimlar.
// sistemdeki gorevi: Moduller arasinda kapsam seviyesinin ortak sozlesmesini saglar; kalici tablo veya token claim'i gerektirmez.
public enum ScopeLevel
{
    SuperAdmin = 1,
    Tenant = 2,
    Organization = 3,
    SubOrganization = 4
}
