namespace Byd.CoreExtensions.Models.Elasticsearch;

/// <summary>
/// Onbellege alinan arama sonucunun JSON hali.
/// </summary>
// islevi: Arama sonucunu belgeleri yazan seri hale getiriciyle uretilmis metin olarak tasir.
// sistemdeki gorevi: Belgeler ic setter'li ev seklinde olabildigi icin sonuc onbellege nesne olarak degil, istemcinin okuyabildigi
// JSON olarak yazilir; boylece onbellekten donen belge indeksten donenle ayni olur.
public class ElasticsearchSearchCacheItem
{
    // Arama sonucunun JSON metni.
    public string Json { get; set; } = default!;
}
