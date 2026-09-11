using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Belirli bir konuya bagli olmayan genel is hatasi kodlari.
/// </summary>
// islevi: Modul genelinde gecerli hata kodlarini derleme guvenli sabitler olarak tanimlar.
// sistemdeki gorevi: BusinessException'larin ve localization kayitlarinin ortak anahtar kaynagi; konuya ozel kodlar kendi ExceptionCodes ailesinde yasar.
public static class GeneralExceptionCodes
{
    // Baska kayitlarla iliskili oldugu icin silinemeyen veri.
    public const string RelationalDataDelete = ExceptionCodeConsts.GeneralGroup + "0001";

    // Filtre isteginde taninmayan operator belirteci gonderildi.
    public const string UnsupportedFilterOperator = ExceptionCodeConsts.GeneralGroup + "0002";

    // Filtre degeri hedef CLR tipine donusturulemedi.
    public const string InvalidFilterValue = ExceptionCodeConsts.GeneralGroup + "0003";

    // Sayfa numarasi desteklenen araligin disinda gonderildi.
    public const string InvalidPage = ExceptionCodeConsts.GeneralGroup + "0004";

    // Sayfa boyutu desteklenen araligin disinda gonderildi.
    public const string InvalidPageSize = ExceptionCodeConsts.GeneralGroup + "0005";

    // Predicate ile aranan kayit veritabaninda bulunamadi.
    public const string EntityNotFound = ExceptionCodeConsts.GeneralGroup + "0006";

    // Ayni degerlere sahip bir kayit zaten var oldugu icin islem yapilamadi.
    public const string AlreadyExists = ExceptionCodeConsts.GeneralGroup + "0007";

    // Zorunlu bir alan bos birakildi.
    public const string RequiredValueMissing = ExceptionCodeConsts.GeneralGroup + "0008";

    // Enum alanina tanimli olmayan bir deger gonderildi.
    public const string InvalidEnumValue = ExceptionCodeConsts.GeneralGroup + "0009";

    // Rate limit asildi; istemci pencere/kota dolduktan sonra yeniden denemelidir.
    public const string TooManyRequests = ExceptionCodeConsts.GeneralGroup + "0010";
}
