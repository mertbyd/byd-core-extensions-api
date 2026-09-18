using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Redis baglantisi hata kodlarini tanimlar.
/// </summary>
// islevi: Redis baglantisinin kurulamadigi durumlara kararli kod verir.
// sistemdeki gorevi: Baglanti kaydi ve localization icin tek hata sozlesmesi sahibidir.
public static class RedisExceptionCodes
{
    public const string ConnectionNotConfigured = ExceptionCodeConsts.RedisGroup + "0001";
}
