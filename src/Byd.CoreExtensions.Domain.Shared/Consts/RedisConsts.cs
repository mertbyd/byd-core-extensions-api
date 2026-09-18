namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Redis baglantisinin host yapilandirmasindaki anahtarlarini tanimlar.
/// </summary>
// islevi: Redis'in acik olup olmadigini ve baglanti adresini tasiyan yapilandirma anahtarlarini tek yerde tutar.
// sistemdeki gorevi: Modulun Redis kaydi ile ABP'nin Redis cache modulunun host'ta ayni bolumu okumasini saglar.
public static class RedisConsts
{
    public const string IsEnabledKey = "Redis:IsEnabled";
    public const string ConfigurationKey = "Redis:Configuration";
}
