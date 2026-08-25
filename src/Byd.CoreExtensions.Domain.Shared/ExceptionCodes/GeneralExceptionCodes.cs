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

    // Filtre degeri hedef CLR tipine donusturulemedi.
    public const string InvalidFilterValue = Prefix + ":00003";

    // Sayfa numarasi desteklenen araligin disinda gonderildi.
    public const string InvalidPage = Prefix + ":00004";

    // Sayfa boyutu desteklenen araligin disinda gonderildi.
    public const string InvalidPageSize = Prefix + ":00005";

    // Predicate ile aranan kayit veritabaninda bulunamadi.
    public const string EntityNotFound = Prefix + ":00006";

    // Ayni degerlere sahip bir kayit zaten var oldugu icin islem yapilamadi.
    public const string AlreadyExists = Prefix + ":00007";

    // Zorunlu bir alan bos birakildi.
    public const string RequiredValueMissing = Prefix + ":00008";

    // Enum alanina tanimli olmayan bir deger gonderildi.
    public const string InvalidEnumValue = Prefix + ":00009";
}
