using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Belirli bir konuya bagli olmayan genel is hatasi kodlari.
/// </summary>
// islevi: Modul genelinde gecerli hata kodlarini derleme guvenli sabitler olarak tanimlar.
// sistemdeki gorevi: BusinessException'larin ve localization kayitlarinin ortak anahtar kaynagi; konuya ozel kodlar kendi ExceptionCodes ailesinde yasar.
public static class GeneralExceptionCodes
{
    // Tum genel kodlarin ortak on eki; gramer "<Modul>.<Konu>:<sira>" seklindedir.
    public const string Prefix = ExceptionCodeConsts.RootNamespace + ".General";
    // Baska kayitlarla iliskili oldugu icin silinemeyen veri.
    public const string RelationalDataDelete = Prefix + ":00001";

    // Filtre isteginde taninmayan operator belirteci gonderildi.
    public const string UnsupportedFilterOperator = Prefix + ":00002";
}
