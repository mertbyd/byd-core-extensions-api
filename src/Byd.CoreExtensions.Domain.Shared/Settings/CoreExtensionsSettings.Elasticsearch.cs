namespace Byd.CoreExtensions.Settings;

/// <summary>
/// Elasticsearch baglanti ayarlarinin adlarini tanimlar.
/// </summary>
// islevi: Kume adresinin, kimlik bilgisinin, acik/kapali durumunun ve istek izleme kipinin ayar adlarini tanimlar.
// sistemdeki gorevi: Istemci saglayicisi, saglik yoklamasi ve sistem log akisi ayar adini buradan alir; inline string kullanilmaz.
public partial class CoreExtensionsSettings
{
    public static class Elasticsearch
    {
        public const string Default = GroupName + ".Elasticsearch";

        // Kumenin adresi.
        public const string Url = Default + ".Url";

        // Temel kimlik dogrulamasinin kullanici adi; bossa istek kimliksiz gider.
        public const string UserName = Default + ".UserName";

        // Temel kimlik dogrulamasinin parolasi.
        public const string Password = Default + ".Password";

        // Elasticsearch'in kullanima acik olup olmadigi; kapaliyken istemci istenemez.
        public const string IsEnabled = Default + ".IsEnabled";

        // Acikken istek ve cevap govdeleri hata ayrintisinda tutulur; yalniz sorun ararken acilir.
        public const string IsTraceMode = Default + ".IsTraceMode";
    }
}
