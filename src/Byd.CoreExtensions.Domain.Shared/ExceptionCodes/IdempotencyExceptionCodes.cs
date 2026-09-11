using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Idempotency akisina ozgu is hatasi kodlari.
/// </summary>
// islevi: Ayni istegin kisa sure icinde tekrar gonderildigi durumu derleme guvenli sabit olarak tanimlar.
// sistemdeki gorevi: Idempotency filtresinin urettigi hata yanitinin ve localization kaydinin anahtar kaynagi.
public static class IdempotencyExceptionCodes
{
    // Ayni istek, saklama suresi dolmadan tekrar gonderildi.
    public const string DuplicateRequest = ExceptionCodeConsts.IdempotencyGroup + "0001";
}
