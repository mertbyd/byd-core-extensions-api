using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Settings;

// islevi: Onbellek kullanim ayarlarini ABP ayar tanimlarina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir.
public partial class CoreExtensionsSettingDefinitionProvider
{
    private static void AddCachingSettings(ISettingDefinitionContext context)
    {
        // Iki onbellek de varsayilan olarak aciktir; sorun ayiklarken veri tazeligini gormek icin calisirken kapatilabilir.
        context.Add(
            new SettingDefinition(
                CoreExtensionsSettings.Caching.IsEntityCachingEnabled,
                defaultValue: bool.TrueString,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.Caching.IsResponseCachingEnabled,
                defaultValue: bool.TrueString,
                isVisibleToClients: false));
    }
}
