namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Bir endpoint icin secilebilecek rate limit algoritma belirtecleri.
/// </summary>
// islevi: Rate limit algoritma secimlerini derleme guvenli sabit metinler olarak tanimlar.
// sistemdeki gorevi: Gecerli algoritma kumesinin tek sahibi; oznitelik ve ayar okuyucu gelen metni bu kumeye karsi dogrular, kod hicbir yerde ham algoritma metni tasimaz.
public static class RateLimitAlgorithm
{
    // Sabit pencere: her pencere basinda sayac sifirlanir.
    public const string FixedWindow = "fixedWindow";

    // Kayan pencere: pencere dilimlere bolunerek daha yumusak sinir uygular.
    public const string SlidingWindow = "slidingWindow";

    // Token kova: belirli araliklarla token yenilenir, kisa sureli burst'e izin verir.
    public const string TokenBucket = "tokenBucket";

    // Es zamanlilik: ayni anda ucusta olan istek sayisini sinirlar.
    public const string Concurrency = "concurrency";
}
