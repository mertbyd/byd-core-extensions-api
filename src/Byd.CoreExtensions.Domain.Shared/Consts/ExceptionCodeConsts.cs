namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Hata kodu ad-uzayinin kararli koku.
/// </summary>
// islevi: Modulun butun hata kodlarinin basinda duran ad-uzayi parcasini tanimlar.
// sistemdeki gorevi: Hem konu bazli ExceptionCodes ailelerinin on eki hem de ABP'nin MapCodeNamespace kaydi bu tek degeri okur.
public static class ExceptionCodeConsts
{
    // Modulun hata kodu ad-uzayi koku; gramer "<Kok>.<Konu>:<sira>" seklindedir.
    public const string RootNamespace = "CoreExtensions";
}
