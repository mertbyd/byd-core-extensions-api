using System.Collections.Generic;

namespace Byd.CoreExtensions.Models.Elasticsearch;

/// <summary>
/// Tek arama isteginin sayfa belgelerini ve filtreye uyan toplam belge sayisini tasir.
/// </summary>
// islevi: Liste ve sayimi ayni istekten donen sonucu bir arada tutar.
// sistemdeki gorevi: Arama repository'sinin tek istekli arama ve onbellekli arama cevabidir; onbellege de bu sekliyle yazilir.
public class ElasticsearchSearchResult<TDocument>
{
    // Istenen sayfanin belgeleri.
    public List<TDocument> Items { get; set; } = [];

    // Sayfalamadan once filtreye uyan toplam belge sayisi.
    public long TotalCount { get; set; }
}
