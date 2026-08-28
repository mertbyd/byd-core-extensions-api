using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// API anahtari dogrulamasina ve CRUD akisina ozgu is hatasi kodlari.
/// </summary>
// islevi: API anahtari kontrolunun ve girdi dogrulamasinin basarisiz oldugu durumlari derleme guvenli sabitler olarak tanimlar.
// sistemdeki gorevi: API anahtari filtresinin ve validator'larinin urettigi hata yanitlarinin ve localization kayitlarinin ortak anahtar kaynagi.
public static class ApiKeyExceptionCodes
{
    // Tum API anahtari kodlarinin ortak on eki; gramer "<Kok>.ApiKey:<sira>" seklindedir.
    public const string Prefix = ExceptionCodeConsts.RootNamespace + ".ApiKey";

    // Istekte API anahtari header'i hic gonderilmedi.
    public const string MissingApiKey = Prefix + ":00001";

    // Gonderilen anahtar bilinen hicbir kayitla eslesmiyor.
    public const string InvalidApiKey = Prefix + ":00002";

    // Anahtarin gecerlilik suresi dolmus.
    public const string ExpiredApiKey = Prefix + ":00004";

    // Anahtar adi bos gonderildi.
    public const string NameRequired = Prefix + ":00005";

    // Anahtar adi izin verilen uzunlugu asiyor.
    public const string NameTooLong = Prefix + ":00006";
}
