namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit global varsayilanlarinin belirli bir versiyondaki degismez anlik goruntusu.
/// </summary>
// islevi: Etkinlik, algoritma, istek limiti, pencere ve versiyon degerlerini degismez tek nesnede tasir.
// sistemdeki gorevi: Limiter factory'nin senkron okudugu durum sozlesmesi; versiyon degistikce partition anahtari degisir ve taze limiter kurulur.
public sealed class RateLimitSnapshot
{
    public RateLimitSnapshot(
        bool isEnabled,
        string algorithm,
        int permitLimit,
        int windowSeconds,
        long version)
    {
        IsEnabled = isEnabled;
        Algorithm = algorithm;
        PermitLimit = permitLimit;
        WindowSeconds = windowSeconds;
        Version = version;
    }

    // Rate limit uygulamasinin modul icin etkin olup olmadigini belirtir.
    public bool IsEnabled { get; }

    // Aktif algoritma belirteci (RateLimitAlgorithm sabitleri).
    public string Algorithm { get; }

    // Pencere basina izin verilen istek sayisi.
    public int PermitLimit { get; }

    // Pencere uzunlugu (saniye).
    public int WindowSeconds { get; }

    // Bu anlik goruntunun versiyonu; her guncellemede artar.
    public long Version { get; }
}
