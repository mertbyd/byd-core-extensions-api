namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Saglik ucunun rotasini ve yoklama adlarini tanimlar.
/// </summary>
// islevi: Saglik ucunun adresini ve her bagimlilik yoklamasinin rapordaki adini tek yerde tutar.
// sistemdeki gorevi: Yoklama kayitlari ve uc eslemesi ayni adlari kullanir.
public static class HealthCheckConsts
{
    // Saglik raporunun okundugu rota.
    public const string Route = "/health";

    // PostgreSQL yoklamasinin adi.
    public const string DatabaseName = "database";

    // Redis yoklamasinin adi.
    public const string RedisName = "redis";

    // RabbitMQ yoklamasinin adi.
    public const string RabbitMqName = "rabbitmq";

    // Elasticsearch yoklamasinin adi.
    public const string ElasticsearchName = "elasticsearch";

    // Yoklamasi kapali tutulan bagimligin raporda gorunen aciklamasi.
    public const string DisabledDescription = "disabled";
}
