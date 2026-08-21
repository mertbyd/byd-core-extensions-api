namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Sayfalama yuzeyine ek olarak tipli filtre modeli tasiyan data table istegi sozlesmesi.
/// </summary>
// islevi: Istegin, tuketicinin kendi tanimladigi filtre modelini tasidigini garanti eder.
// sistemdeki gorevi: Filtre modelini jenerik parametreye baglar; boylece her tablo kendi filtre kumesini Core'a hicbir sey eklemeden tanimlar ve Core hicbir is bilgisi tasimaz.
public interface IDataTablePagingProperty<TFilters> : IDataTableProperty
    where TFilters : class
{
    // Tabloya ozel filtre modeli; gonderilmezse filtre uygulanmaz.
    TFilters? Filters { get; set; }
}
