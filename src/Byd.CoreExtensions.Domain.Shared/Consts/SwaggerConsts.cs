namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Swagger dokumaninin adini, basligini ve XML aciklama dosyalarini tanimlar.
/// </summary>
// islevi: Swagger dokuman kimligi ile derleme ciktisina birakilan XML aciklama dosya adlarini tek yerde tutar.
// sistemdeki gorevi: Host'un Swagger kompozisyonunun tek string kaynagidir; dosya adi projeyle birebir eslesmezse
// aciklamalar sessizce kaybolacagi icin bu degerler magic string olarak birakilmaz.
public static class SwaggerConsts
{
    // Uretilen Swagger dokumaninin adi.
    public const string DocumentName = "v1";

    // Swagger UI'da gorunen dokuman basligi.
    public const string DocumentTitle = "CoreExtensions API";

    // Swagger UI'nin dokumani okudugu adres.
    public const string DocumentUrl = "/swagger/v1/swagger.json";

    // Controller aciklamalarini tasiyan HttpApi projesinin XML cikti dosyasi.
    public const string HttpApiXmlDocumentationFileName = "Byd.CoreExtensions.HttpApi.xml";

    // DTO aciklamalarini tasiyan Application.Contracts projesinin XML cikti dosyasi.
    public const string ApplicationContractsXmlDocumentationFileName =
        "Byd.CoreExtensions.Application.Contracts.xml";
}
