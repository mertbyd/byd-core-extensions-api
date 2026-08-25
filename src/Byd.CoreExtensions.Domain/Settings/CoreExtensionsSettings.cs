namespace Byd.CoreExtensions.Settings;

/// <summary>
/// Modul ayarlarinin derleme guvenli isim sabitleri.
/// </summary>
// islevi: Ayar adlarini derleme guvenli sabitler olarak tanimlar; rate limit global varsayilanlarinin isimlerini tutar.
// sistemdeki gorevi: SettingDefinitionProvider ve ISettingProvider okumalarinin ortak isim kaynagi; ayar adlari hicbir yerde ham metin olarak tekrar etmez.
public static class CoreExtensionsSettings
{
    public const string GroupName = "CoreExtensions";

    // Rate limit global varsayilan algoritma ayarinin adi.
    public const string RateLimitDefaultAlgorithm = GroupName + ".RateLimiting.DefaultAlgorithm";

    // Rate limit global varsayilan istek limiti ayarinin adi.
    public const string RateLimitDefaultPermitLimit = GroupName + ".RateLimiting.DefaultPermitLimit";

    // Rate limit global varsayilan pencere uzunlugu (saniye) ayarinin adi.
    public const string RateLimitDefaultWindowSeconds = GroupName + ".RateLimiting.DefaultWindowSeconds";
}
