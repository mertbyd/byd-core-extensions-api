namespace Byd.CoreExtensions.Consts;

/// <summary>
/// DataTable sorgu ifadeleri kurulurken kullanilan sabit metin tokenlari.
/// </summary>
// islevi: Expression lambdalarinin parametre adlarini, property yolu ayiracini ve hata verisi anahtarlarini tek yerde tutar.
// sistemdeki gorevi: Filtre/siralama/sayfalama ureten uzanti ve repository kodunun magic string tekrar etmesini onler.
public static class DataTableQueryConsts
{
    // Entity uzerinde kurulan filtre/siralama lambdalarinin parametre adi.
    public const string EntityParameterName = "entity";

    // Iliski kontrolunde bagimli entity uzerinde kurulan lambdanin parametre adi.
    public const string DependentParameterName = "dependent";

    // Noktali property yolunu parcalarina ayiran ayirac.
    public const char PropertyPathSeparator = '.';

    // BusinessException verisine eklenen operator anahtari.
    public const string OperatorDataKey = "operator";

    // BusinessException verisine eklenen hedef tip anahtari.
    public const string TargetTypeDataKey = "targetType";

    // BusinessException verisine eklenen ham deger anahtari.
    public const string ValueDataKey = "value";

    // BusinessException verisine eklenen sayfa numarasi anahtari.
    public const string PageDataKey = "page";

    // BusinessException verisine eklenen sayfa boyutu anahtari.
    public const string PageSizeDataKey = "pageSize";
}
