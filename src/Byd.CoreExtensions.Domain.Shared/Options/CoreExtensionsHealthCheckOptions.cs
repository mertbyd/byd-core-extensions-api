namespace Byd.CoreExtensions.Options;

/// <summary>
/// Host'un saglik ucunda hangi bagimliliklari yoklayacagini tasir.
/// </summary>
// islevi: Saglik ucunu ve veritabani, Redis, RabbitMQ, Elasticsearch yoklamalarini tek tek acip kapatir.
// sistemdeki gorevi: Modul hepsini acik getirir; o bagimliligi kullanmayan host kendi modulunde PreConfigure ile kapatir.
public class CoreExtensionsHealthCheckOptions
{
    // Saglik ucunun tamami; kapaliysa hicbir yoklama kaydedilmez ve uc acilmaz.
    public bool IsHealthCheckEnabled { get; set; } = true;

    // Default baglanti dizesindeki PostgreSQL yoklamasi.
    public bool IsDatabaseHealthCheckEnabled { get; set; } = true;

    // Redis yoklamasi; host Redis'i kapattiysa yine de kaydedilmez.
    public bool IsRedisHealthCheckEnabled { get; set; } = true;

    // Elasticsearch yoklamasi; ayarlarda kapali olan Elasticsearch saglikli sayilir.
    public bool IsElasticsearchHealthCheckEnabled { get; set; } = true;

    // RabbitMQ yoklamasi.
    public bool IsRabbitMqHealthCheckEnabled { get; set; } = true;
}
