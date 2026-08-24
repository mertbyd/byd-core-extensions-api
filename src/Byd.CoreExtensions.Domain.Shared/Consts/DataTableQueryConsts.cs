namespace Byd.CoreExtensions.Consts;

/// <summary>
/// DataTable sorgu ifadeleri kurulurken kullanilan sabit parametre adlari.
/// </summary>
// islevi: Expression agaclarindaki lambda parametrelerinin adlarini tek yerde tutar.
// sistemdeki gorevi: Filtre ve iliski sorgularini ureten uzanti/repository kodunun ayni parametre adini tekrar yazmasini onler.
public static class DataTableQueryConsts
{
    // Entity uzerinde kurulan filtre/siralama lambdalarinin parametre adi.
    public const string EntityParameterName = "entity";

    // Iliski kontrolunde bagimli entity uzerinde kurulan lambdanin parametre adi.
    public const string DependentParameterName = "dependent";
}
