using System;
namespace Byd.CoreExtensions.Attributes;
/// <summary>
/// Bir controller veya action icin rate limit davranisini inline bildiren oznitelik.
/// </summary>
// islevi: Endpoint'in algoritmasini, partition stratejisini ve sayisal limitlerini oznitelik uzerinde tasir; belirtilmeyen degerler global ayardan cozulur.
// sistemdeki gorevi: Rate limit ozellestirmesinin kaynagi; oznitelik her zaman kazanir, boylece bir endpoint global varsayilandan bagimsiz kendi limitini belirleyebilir.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RateLimitAttribute : Attribute
{
    // Uygulanacak algoritma belirteci (RateLimitAlgorithm sabitleri); null birakilirsa global ayardan cozulur.
    public string? Algorithm { get; set; }

    // Sayacin bolunecegi strateji belirteci (RateLimitPartitionStrategy sabitleri); null birakilirsa IP'ye duser.
    public string? PartitionStrategy { get; set; }

    // Pencere basina izin verilen istek sayisi; 0 birakilirsa global ayardan cozulur.
    public int PermitLimit { get; set; }

    // Pencere uzunlugu (saniye); 0 birakilirsa global ayardan cozulur.
    public int WindowSeconds { get; set; }

    // Kayan pencerenin bolunecegi dilim sayisi (yalnizca SlidingWindow); 0 birakilirsa sabit varsayilan kullanilir.
    public int SegmentsPerWindow { get; set; }

    // Token kovada her periyotta eklenecek token sayisi (yalnizca TokenBucket); 0 birakilirsa PermitLimit kadar yenilenir.
    public int TokensPerPeriod { get; set; }

    // Limit dolduktan sonra kuyruga alinacak istek sayisi; 0 kuyruk kullanilmaz demektir.
    public int QueueLimit { get; set; }
}
