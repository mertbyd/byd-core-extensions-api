namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Hata kodu ad-uzayinin kararli koku ve aile gruplari.
/// </summary>
// islevi: Modulun butun hata kodlarinin basinda duran ad-uzayini ve her ailenin grup numarasini
// tanimlar.
// sistemdeki gorevi: Hem ExceptionCodes ailelerinin on eki hem de ABP'nin MapCodeNamespace kaydi
// bu tek degeri okur.
//
// NEDEN AD-UZAYI DUZ. ABP hata mesajini ararken kodun iki noktadan onceki tamamini alir
// (Code.Split(':')[0]) ve kayit sozlugunde tam eslesme arar. Ad-uzayina aile eklenirse
// (ornek "CoreExtensions.General") kok kaydi eslesmez ve BusinessException mesaji sessizce
// "An internal error occurred" metnine duser. Bu yuzden ad-uzayi tektir, gruplama numarada
// tasinir; ABP'nin kendi modulleri de ayni sekilde yazar: "Volo.Abp.Identity:010002".
public static class ExceptionCodeConsts
{
    // Modulun hata kodu ad-uzayi koku; gramer "<Kok>:<GG><SSSS>" seklindedir.
    public const string RootNamespace = "CoreExtensions";

    // Konuya bagli olmayan genel kodlar.
    public const string GeneralGroup = RootNamespace + ":01";

    // API anahtari akisi.
    public const string ApiKeyGroup = RootNamespace + ":02";

    // Idempotency akisi.
    public const string IdempotencyGroup = RootNamespace + ":03";

    // Modul ayari akisi.
    public const string ModuleSettingGroup = RootNamespace + ":04";

    // E-posta sablonu akisi.
    public const string EmailTemplateGroup = RootNamespace + ":05";
}
