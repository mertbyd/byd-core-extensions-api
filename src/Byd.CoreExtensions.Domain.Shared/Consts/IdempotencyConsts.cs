namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Idempotency tekrar penceresinin sabit degerlerini tanimlar.
/// </summary>
// islevi: Ayni istegin cache'te ne kadar tutulacagini ve kabul edilen sure araligini tek yerde tutar.
// sistemdeki gorevi: ModuleSetting validation, EF konfigurasyonu ve idempotency filtresinin ortak sure sozlesmesidir.
public static class IdempotencyConsts
{
    // Imzasi hesaplanacak istek govdesinin ust siniri (bayt).
    public const int MaxBodyBytes = 1024 * 1024;

    // Ayni istegin cache'te tutulacagi varsayilan sure (saniye).
    public const int DefaultTtlSeconds = 300;

    // Ayarda kabul edilen en kisa ve en uzun tekrar penceresi.
    public const int MinTtlSeconds = 1;
    public const int MaxTtlSeconds = 86400;
}
