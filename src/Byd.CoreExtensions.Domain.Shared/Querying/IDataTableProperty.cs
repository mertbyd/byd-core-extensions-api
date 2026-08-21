using System.Collections.Generic;

namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Sayfalama ve siralama yuzeyini garanti eden temel data table istegi sozlesmesi.
/// </summary>
// islevi: Bir istegin sayfa numarasi, sayfa boyutu ve siralama adimlari tasidigini bildirir.
// sistemdeki gorevi: Motorun filtre tipini bilmeden sayfalama ve siralamaya erisebilmesini saglayan jenerik olmayan taban; DataTablePaging'den turemeyen istek modelleri de bu sozu uygulayarak sisteme girer.
public interface IDataTableProperty
{
    // Istenen sayfa numarasi; ilk sayfa 1'dir. Varsayilan degeri uygulayan sinif belirler.
    int Page { get; set; }

    // Bir sayfada dondurulecek kayit sayisi. Varsayilan degeri uygulayan sinif belirler.
    int PageSize { get; set; }

    // Uygulanacak siralama adimlari; listedeki sira siralama onceligini belirler.
    List<DataTableSorting>? Sorting { get; set; }
}
