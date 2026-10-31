namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Filtre isteginde kullanilabilecek operator belirtecleri.
/// </summary>
// islevi: Tel uzerinde tasinan operator metinlerini derleme guvenli sabitler olarak tanimlar.
// sistemdeki gorevi: Gecerli operator kumesinin tek sahibi; motor gelen metni bu kumeye karsi dogrular ve kod hicbir yerde ham operator metni tasimaz.
public static class DataTableFilterOperator
{
    // Deger alanina tam esitlik.
    public const string Equal = "==";

    // Deger alanina esit olmayan kayitlar.
    public const string NotEqual = "!=";

    // Deger alanindan buyuk olan kayitlar.
    public const string GreaterThan = ">";

    // Deger alanindan buyuk veya ona esit kayitlar.
    public const string GreaterThanOrEqual = ">=";

    // Deger alanindan kucuk olan kayitlar.
    public const string LessThan = "<";

    // Deger alanindan kucuk veya ona esit kayitlar.
    public const string LessThanOrEqual = "<=";

    // Metin alaninda gecme kontrolu; sembolu olmadigi icin kelime kullanilir.
    public const string Contains = "contains";

    // Metin alaninda gecmeme kontrolu; Contains'in tersidir, bos alan da gecmiyor sayilir.
    public const string NotContains = "notContains";

    // Arama indeksinde kelime bazli tam metin eslesmesi; veritabani sorgusunda desteklenmez.
    public const string Match = "match";

    // Arama indeksinde yazim hatasina toleransli eslesme; veritabani sorgusunda desteklenmez.
    public const string Fuzzy = "fuzzy";

    // Value ve SecondValue arasindaki kapali aralik; SecondValue'yu kullanan tek operator.
    public const string Between = "between";

    // Values listesindeki degerlerden herhangi birine esitlik; Values'i kullanan tek operator.
    public const string In = "in";

    // Nullable veya referans tipli alanda deger bulunmamasini denetler.
    public const string IsNull = "isNull";

    // Nullable veya referans tipli alanda deger bulunmasini denetler.
    public const string IsNotNull = "isNotNull";
}
