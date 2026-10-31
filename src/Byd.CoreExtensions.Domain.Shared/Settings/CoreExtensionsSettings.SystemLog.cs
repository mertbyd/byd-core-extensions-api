namespace Byd.CoreExtensions.Settings;

/// <summary>
/// Host'un sistem loglarini Elasticsearch'e yazma ayarlarinin adlarini tanimlar.
/// </summary>
// islevi: Log akisinin acik/kapali durumunun, veri akisi adinin, en dusuk seviyenin, akis tanilarinin ve yeniden okuma
// araliginin ayar adlarini tanimlar.
// sistemdeki gorevi: Sistem log iscisi ayarlari bu adlarla okur; ayar degisince host yeniden baslatilmadan log akisi degisir.
public partial class CoreExtensionsSettings
{
    public static class SystemLog
    {
        public const string Default = GroupName + ".SystemLog";

        // Sistem loglarinin Elasticsearch'e de yazilip yazilmayacagi.
        public const string IsElasticsearchEnabled = Default + ".IsElasticsearchEnabled";

        // Veri akisi adinin orta parcasi; bossa host'un modul adi kullanilir.
        public const string IndexName = Default + ".IndexName";

        // Elasticsearch'e yazilan en dusuk log seviyesi.
        public const string MinimumLevel = Default + ".MinimumLevel";

        // Acikken log akisinin kendi tani mesajlari konsola yazilir.
        public const string IsSelfLogEnabled = Default + ".IsSelfLogEnabled";

        // Gonderilemeyen log paketlerinin hata ayrintisinin eklendigi dosyanin yolu; bossa yazilmaz.
        public const string FailureLogPath = Default + ".FailureLogPath";

        // Ayarlarin yeniden okunma araligi (saniye).
        public const string RefreshIntervalSeconds = Default + ".RefreshIntervalSeconds";
    }
}
