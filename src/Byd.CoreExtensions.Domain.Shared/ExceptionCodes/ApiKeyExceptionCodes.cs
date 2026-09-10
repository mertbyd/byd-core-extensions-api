using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// API anahtari dogrulamasina ve CRUD akisina ozgu is hatasi kodlari.
/// </summary>
// islevi: API anahtari kontrolunun ve girdi dogrulamasinin basarisiz oldugu durumlari derleme guvenli sabitler olarak tanimlar.
// sistemdeki gorevi: API anahtari filtresinin ve validator'larinin urettigi hata yanitlarinin ve localization kayitlarinin ortak anahtar kaynagi.
public static class ApiKeyExceptionCodes
{
    // Istekte API anahtari header'i hic gonderilmedi.
    public const string MissingApiKey = ExceptionCodeConsts.ApiKeyGroup + "0001";

    // Gonderilen anahtar bilinen hicbir kayitla eslesmiyor.
    public const string InvalidApiKey = ExceptionCodeConsts.ApiKeyGroup + "0002";

    // Anahtarin gecerlilik suresi dolmus.
    public const string ExpiredApiKey = ExceptionCodeConsts.ApiKeyGroup + "0004";

    // Anahtar adi bos gonderildi.
    public const string NameRequired = ExceptionCodeConsts.ApiKeyGroup + "0005";

    // Anahtar adi izin verilen uzunlugu asiyor.
    public const string NameTooLong = ExceptionCodeConsts.ApiKeyGroup + "0006";
}
