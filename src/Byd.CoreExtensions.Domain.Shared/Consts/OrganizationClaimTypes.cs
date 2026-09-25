namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Organizasyon kapsaminin token uzerinde tasindigi claim adlari.
/// </summary>
// islevi: Kapsami token'a yazan taraf ile istekte okuyan tarafin ayni claim adini kullanmasini saglar.
// sistemdeki gorevi: Organizasyon kapsaminin tek sabit kaynagi. Adlar burada durur cunku token'i ureten modul
// core'a bagimlidir; ters yonde bagimlilik olmadigi icin iki taraf metni ancak bu katmanda paylasabilir.
public static class OrganizationClaimTypes
{
    // Istegin hangi organizasyon adina calistigini bildiren claim.
    public const string OrganizationId = "organization_id";

    // Organizasyonun okunabilir adi. Kapsamla birlikte tasinir cunku okuma aninda adi cozecek bir organizasyon
    // kaynagi yoktur; ada ihtiyaci olan kayit onu yazma aninda kendi icine gecirir.
    public const string OrganizationName = "organization_name";
}
