namespace Byd.CoreExtensions.Consts;

/// <summary>
/// API anahtari dogrulamasinin ve CRUD akisinin sabit degerleri.
/// </summary>
// islevi: Istekten anahtarin okunacagi header adini, tablo adini, kolon uzunluklarini ve uretilen anahtarin entropi boyutunu tek yerde tutar.
// sistemdeki gorevi: API anahtari altyapisinin tek sabit kaynagi; filtre, EF konfigurasyonu, manager ve validator ayni degerleri buradan alir.
public static class ApiKeyConsts
{
    // Istegin API anahtarini tasidigi HTTP header adi.
    public const string HeaderName = "X-Api-Key";

    // API anahtari tablosunun (on ek haric) adi.
    public const string TableName = "ApiKeys";

    // Anahtar deger kolonunun azami uzunlugu.
    public const int MaxKeyLength = 128;

    // Anahtar adi kolonunun azami uzunlugu.
    public const int MaxNameLength = 128;

    // Uretilen anahtarin ham entropi boyutu (bayt); base64url kodlamasi sonrasi 43 karaktere karsilik gelir.
    public const int GeneratedKeyByteLength = 32;

    // Bellekteki anahtar goruntusunun tazelik suresi (saniye). Sure dolunca sonraki dogrulama
    // tabloyu bastan okur; silinen veya suresi degistirilen anahtarlar bu surede sisteme yansir.
    public const int CacheTtlSeconds = 60;
}
