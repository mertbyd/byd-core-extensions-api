using System.Collections.Generic;

namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Sayfalama ve siralama bilgisini tasiyan somut data table istegi tabani.
/// </summary>
// islevi: Sayfa numarasi, sayfa boyutu ve siralama listesini varsayilanlariyla birlikte tasir.
// sistemdeki gorevi: Tabloya ozel istek modellerinin turedigi ortak taban; sayfa boyutu sinirlarinin sahibi de burasidir, sinirlari zorlayan dogrulama motor katmaninda yazilir.
public class DataTablePaging : IDataTableProperty
{
    // Sayfalamanin ilk sayfa numarasi; atlanacak kayit sayisi bu degere gore hesaplanir.
    public const int FirstPage = 1;

    // Sayfa boyutu gonderilmediginde kullanilacak varsayilan.
    public const int DefaultPageSize = 10;
    // Tek istekte donebilecek en buyuk sayfa boyutu; dogrulama bu siniri zorlar.
    public const int MaxPageSize = 1000;
    // Ilk sayfa 1 kabul edilir; sifir veya negatif deger atlanacak kayit sayisini negatife dusurur ve dogrulamada reddedilir.
    public int Page { get; set; } = FirstPage;
    // Sayfa boyutu; ust sinir MaxPageSize'dir.
    public int PageSize { get; set; } = DefaultPageSize;
    // Uygulanacak siralama adimlari; listedeki sira siralama onceligini belirler.
    public List<DataTableSorting>? Sorting { get; set; }
}
