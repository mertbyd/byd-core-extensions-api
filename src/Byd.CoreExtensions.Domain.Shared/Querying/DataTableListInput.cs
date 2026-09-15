namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Tipli filtre modeli, siralama ve sayfalama bilgisini tek istekte tasiyan hazir liste girdisi.
/// </summary>
// islevi: DataTablePaging'i tabloya ozel filtre modeliyle birlestirir.
// sistemdeki gorevi: Tuketici moduller her tablo icin ayri liste girdisi sinifi yazmaz; yalniz filtre modelini yazip bu sinifi kapali tipiyle kullanir.
public class DataTableListInput<TFilters> : DataTablePaging, IDataTablePagingProperty<TFilters>
    where TFilters : class
{
    // Tabloya ozel filtre modeli; gonderilmezse filtre uygulanmaz.
    public TFilters? Filters { get; set; }
}
