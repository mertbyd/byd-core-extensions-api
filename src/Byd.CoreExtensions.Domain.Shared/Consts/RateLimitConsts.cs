using Byd.CoreExtensions.RateLimiting;

namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Rate limit yapisinin sabit varsayilan degerleri.
/// </summary>
// islevi: Global ayar okunamadiginda veya oznitelik ilgili alani atlamadiginda kullanilan sabit varsayilanlari tek yerde tutar.
// sistemdeki gorevi: Rate limit varsayilanlarinin tek kaynagi; setting tanimlari ve Host cozucu ayni degerleri buradan alir, kod hicbir yerde magic sayi tasimaz.
public static class RateLimitConsts
{
    // Ayar okunamadiginda uygulanacak varsayilan algoritma.
    public const string DefaultAlgorithm = RateLimitAlgorithm.FixedWindow;

    // Ayar okunamadiginda pencere basina varsayilan istek limiti.
    public const int DefaultPermitLimit = 100;

    // Ayar okunamadiginda varsayilan pencere uzunlugu (saniye).
    public const int DefaultWindowSeconds = 60;

    // Oznitelik ve ayar belirtmediginde varsayilan kuyruk limiti; 0 kuyruk kullanilmaz demektir.
    public const int DefaultQueueLimit = 0;

    // SlidingWindow'da oznitelik dilim sayisi atmadiginda kullanilan sabit dilim sayisi.
    public const int DefaultSegmentsPerWindow = 6;

    // Global rate limit policy'sinin adi; convention bu adla policy'yi endpoint'lere baglar.
    public const string PolicyName = "CoreExtensions.RateLimit";

    // Partition anahtari parcalarini ayiran ayirac.
    public const string PartitionKeySeparator = ":";

    // Kimlik ve IP bulunamadiginda kullanilan partition kimligi.
    public const string UnknownPartitionKey = "unknown";

    // Modul rate limit'i kapaliyken kullanilan limitsiz partition kimligi.
    public const string NoLimiterPartitionKey = "core-extensions-rate-limit-disabled";
}
