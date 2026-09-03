namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Basarisiz olay kutusunun tablo, indeks ve alan sinirlarini tanimlar.
/// </summary>
// islevi: Basarisiz olay kayitlarinin kalici saklama ve yeniden deneme sabitlerini tek yerde tutar.
// sistemdeki gorevi: Domain, EF konfigurasyonu ve sonraki tuketicilerin ortak sabit sozlesmesidir.
public static class FailedEventInboxConsts
{
    // Basarisiz olay kutusunun on eksiz tablo adi.
    //todo : ayni ailedeki EventInbox ve EventOutbox gibi on eksiz birakildi,
    //       ilerleyen asamada core_extensions_ on eki denenecek; denenip vazgecilirse bu not silinebilir
    public const string TableName = "FailedEventInboxes";

    // Kapsam ve yeniden deneme penceresini destekleyen indeks adi.
    public const string RetryScopeIndex = "IX_FailedEventInboxes_Scope_Retry";

    // Kuyruk mesaj kimligi uzerindeki indeks adi.
    public const string MessageIdIndex = "IX_FailedEventInboxes_MessageId";

    // Kuyruk mesaj kimligi kolonunun azami uzunlugu.
    public const int MaxMessageIdLength = 256;

    // Olay adi kolonunun azami uzunlugu.
    public const int MaxEventNameLength = 256;

    // Son hata mesaji kolonunun azami uzunlugu.
    public const int MaxExceptionMessageLength = 2048;

    // Bir yeniden deneme turunda alinacak varsayilan kayit sayisi.
    public const int DefaultRetryBatchSize = 50;

    // Tek tur icinde arda arda islenebilecek azami parti sayisi.
    public const int MaxRetryRoundsPerRun = 20;

    // Yeniden deneme gecikmesinin ussel artisina temel olan saniye katsayisi.
    public const int RetryBackoffFactorSeconds = 60;

    // Yeniden deneme gecikmesinin ulasabilecegi en yuksek saniye degeri.
    public const int MaxRetryBackoffSeconds = 3600;

    // Basarisiz olay yeniden deneme isleyicisinin tur araligi (saniye).
    public const int RetryProcessorPeriodSeconds = 60;

    // Basarisiz olay yeniden deneme turlarini tek kurulumla sinirlayan dagitik kilit adi.
    public const string DistributedLockName = "CoreExtensions.FailedEventInboxRetry";
}
