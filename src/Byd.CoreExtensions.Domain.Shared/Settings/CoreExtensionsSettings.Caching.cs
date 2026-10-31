namespace Byd.CoreExtensions.Settings;

/// <summary>
/// Onbellek kullaniminin ayar adlarini tanimlar.
/// </summary>
// islevi: Entity listesi onbelleginin ve API yanit onbelleginin acik/kapali durumunun ayar adlarini tanimlar.
// sistemdeki gorevi: Onbellekli repository ve yanit onbellegi filtresi bu adlarla okur; ikisi de calisirken kapatilabilir.
public partial class CoreExtensionsSettings
{
    public static class Caching
    {
        public const string Default = GroupName + ".Caching";

        // Onbellekli repository'lerin listeyi onbellekten okuyup okumayacagi; kapaliyken her okuma tabloya gider.
        public const string IsEntityCachingEnabled = Default + ".IsEntityCachingEnabled";

        // Isaretli action yanitlarinin onbellege alinip alinmayacagi; Redis kapaliyken bu ayar ne olursa olsun kapali sayilir.
        public const string IsResponseCachingEnabled = Default + ".IsResponseCachingEnabled";
    }
}
