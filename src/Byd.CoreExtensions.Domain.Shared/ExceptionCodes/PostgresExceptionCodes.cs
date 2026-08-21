namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Kullaniciya donusturulen PostgreSQL SQLSTATE kodlari.
/// </summary>
// islevi: Postgres'in standart SQLSTATE degerlerini isimlendirilmis sabitler olarak tanimlar.
// sistemdeki gorevi: PostgresExceptionLocalizationKeyProvider'in cozdugu anahtarlarin sahibi; kodlar hem burada hem localization JSON'unda ayni degerle yasar.
public static class PostgresExceptionCodes
{
    // Girilen metin sutun uzunlugunu asti.
    public const string StringDataRightTruncation = "22001";
    // Zorunlu sutun bos birakildi.
    public const string NotNullViolation = "23502";
    // Yabanci anahtar kisiti ihlal edildi.
    public const string ForeignKeyViolation = "23503";
    // Tekillik kisiti ihlal edildi.
    public const string UniqueViolation = "23505";
    // Check kisiti ihlal edildi.
    public const string CheckViolation = "23514";
    // Dislama kisiti ihlal edildi; deger mevcut bir kayitla cakisiyor.
    public const string ExclusionViolation = "23P01";
}
