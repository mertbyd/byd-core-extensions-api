using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Elasticsearch erisimine ozgu is hatasi kodlari.
/// </summary>
// islevi: Kapali Elasticsearch'e erisim denemesini ve kismen reddedilen toplu yazmayi derleme guvenli sabit olarak tanimlar.
// sistemdeki gorevi: Istemci saglayicisinin ve arama repository'sinin urettigi hata yanitinin ve localization kaydinin
// anahtar kaynagi.
public static class ElasticsearchExceptionCodes
{
    // Elasticsearch ayarlarda kapali oldugu halde istemci istendi.
    public const string NotEnabled = ExceptionCodeConsts.ElasticsearchGroup + "0001";

    // Toplu yazmada en az bir belge reddedildi.
    public const string BulkWriteFailed = ExceptionCodeConsts.ElasticsearchGroup + "0002";
}
