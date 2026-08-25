namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Toplu yazma islemlerinin kararli varsayilan degerleri.
/// </summary>
// islevi: Toplu islemlerde tek seferde veritabanina gonderilecek kayit sayisinin varsayilanini tanimlar.
// sistemdeki gorevi: Cagirici batch boyutu belirtmediginde kullanilan tek deger sahibi; sozlesme ve uygulama ayni sabiti okur.
public static class BulkOperationConsts
{
    // Cagirici deger vermediginde kullanilacak batch boyutu.
    public const int DefaultBatchSize = 2000;
}
