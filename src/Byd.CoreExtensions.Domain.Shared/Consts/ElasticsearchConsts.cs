using System;

namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Elasticsearch istemcisinin ve arama repository'sinin kararli degerleri.
/// </summary>
// islevi: Varsayilan indeksi, arama sinirlarini, alan adi son ekini ve bulanik arama toleransini tanimlar.
// sistemdeki gorevi: Istemci saglayicisi, repository ve sorgu cevirici ayni degerleri bu tek kaynaktan okur.
public static class ElasticsearchConsts
{
    // Indeks adi verilmeyen cagrilarin yazdigi ve okudugu indeks.
    public const string DefaultIndex = "default";

    // Tek aramada donebilecek en fazla belge; sayfasiz liste ve tum belgeleri okumanin parca boyu bu sinirla calisir.
    public const int MaxDocumentCount = 10000;

    // Metin alaninin tam eslesme, siralama ve joker arama icin acilan alt alani.
    public const string KeywordSuffix = ".keyword";

    // Joker aramada herhangi bir metni temsil eden karakter.
    public const char WildcardAny = '*';

    // Bulanik aramanin yazim hatasi toleransi; Elasticsearch terimin uzunluguna gore 0, 1 ya da 2 harf secer.
    public const string AutoFuzziness = "AUTO";

    // Yazilamayan belge sayisinin hata verisindeki anahtari.
    public const string FailedItemCountDataKey = "failedItemCount";

    // Tum belgeleri okurken scroll baglaminin acik kaldigi sure.
    public static readonly TimeSpan ScrollKeepAlive = TimeSpan.FromMinutes(1);

    // Onbellekli aramanin sonucu bu sure boyunca saklanir; yazmalar onbellegi temizlemez, sonuc en gec bu sure kadar eskir.
    public static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(1);
}
