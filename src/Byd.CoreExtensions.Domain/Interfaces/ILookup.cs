namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Modelin lookup/referans verisi oldugunu bildiren sozlesme.
/// </summary>
// islevi: Lookup kayitlarinin ortak alanlarini tanimlar: zorunlu ad ve istege bagli is kodu.
// sistemdeki gorevi: Lookup tablolarini tek tipte ele alan jenerik sorgu ve servis davranislarinin dayanagi; aktiflikten bagimsizdir, gerektiginde IHasIsActive ile birlikte uygulanir.
public interface ILookup
{
    // Lookup kaydinin gorunen adi; her kayitta bulunmasi zorunludur.
    string Name { get; set; }

    // Is tarafinin kullandigi kod; kod tasimayan lookup'lar oldugu icin nullable birakilmistir.
    int? Code { get; set; }
}
