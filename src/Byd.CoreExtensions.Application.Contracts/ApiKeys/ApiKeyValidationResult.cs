namespace Byd.CoreExtensions.ApiKeys;
/// <summary>
/// API anahtari dogrulamasinin sonucu: basari halinde anahtar goruntusu, aksi halde localize hata kodu.
/// </summary>
// islevi: Dogrulama cagrisinin tek donus tipi; basari/basarisizligi ve varsa cozulmus anahtari ya da hata kodunu birlikte tasir.
// sistemdeki gorevi: Store ile middleware arasindaki dogrulama sonucu sozlesmesi; basarisizlik ApiKeyExceptionCodes kodu ile ifade edilir, ayri bir enum tutulmaz.
public sealed class ApiKeyValidationResult
{
    private ApiKeyValidationResult(string? errorCode, ApiKeyDescriptor? apiKey)
    {
        ErrorCode = errorCode;
        ApiKey = apiKey;
    }
    // Basarisizlik hata kodu (ApiKeyExceptionCodes); null ise dogrulama gecerlidir.
    public string? ErrorCode { get; }
    // Dogrulama gecerliyse cozulmus anahtar goruntusu; aksi halde null.
    public ApiKeyDescriptor? ApiKey { get; }
    // Dogrulamanin gecerli olup olmadigi.
    public bool IsValid => ErrorCode is null;
    // Gecerli sonuc olusturur.
    public static ApiKeyValidationResult Success(ApiKeyDescriptor apiKey)
    {
        return new ApiKeyValidationResult(null, apiKey);
    }
    // Belirtilen hata koduyla basarisiz sonuc olusturur.
    public static ApiKeyValidationResult Fail(string errorCode)
    {
        return new ApiKeyValidationResult(errorCode, null);
    }
}
