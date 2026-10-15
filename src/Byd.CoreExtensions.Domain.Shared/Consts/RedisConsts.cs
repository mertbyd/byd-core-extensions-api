namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Redis baglantisinin host yapilandirmasindaki anahtarlari.
/// </summary>
// islevi: Redis'in acik olup olmadigini, baglanti dizesini ve paylasilan anahtar on ekini okuyan anahtarlari tanimlar.
// sistemdeki gorevi: Modul Redis'i kendi bolumunden kurar; host yalniz bu anahtarlari appsettings'e koyar.
public static class RedisConsts
{
    // Redis'in kullanilip kullanilmayacagi; "true" olmadikca dagitik onbellek bellek icinde kalir.
    public const string IsEnabledKey = "Redis:IsEnabled";

    // StackExchange.Redis baglanti dizesi.
    public const string ConfigurationKey = "Redis:Configuration";

    // Ayni Redis'i paylasan sistemleri ayiran anahtar on eki; ayni sistemin host'lari ayni on eki kullanir ki
    // birinin temizledigi izin ve ayar onbellegi digerlerinde de temizlensin.
    public const string KeyPrefixKey = "Redis:KeyPrefix";

    // Anahtar desenindeki herhangi bir metni temsil eden karakter.
    public const string PatternAny = "*";
}
