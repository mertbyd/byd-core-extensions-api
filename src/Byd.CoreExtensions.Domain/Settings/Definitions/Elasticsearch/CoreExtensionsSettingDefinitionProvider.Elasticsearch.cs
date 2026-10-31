using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Settings;

// islevi: Elasticsearch baglanti ayarlarini ABP ayar tanimlarina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir.
public partial class CoreExtensionsSettingDefinitionProvider
{
    private static void AddElasticsearchSettings(ISettingDefinitionContext context)
    {
        // Elasticsearch varsayilan olarak kapalidir; acilmadan istemci istenirse kapali oldugunu soyleyen hata doner.
        // Baglanti alanlarinin varsayilani bostur: deger ortama gore degisir. isVisibleToClients hepsinde false: grup gizli
        // anahtar tasir, istemciye acilmaz.
        context.Add(
            new SettingDefinition(
                CoreExtensionsSettings.Elasticsearch.IsEnabled,
                defaultValue: bool.FalseString,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.Elasticsearch.Url,
                defaultValue: null,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.Elasticsearch.UserName,
                defaultValue: null,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.Elasticsearch.Password,
                defaultValue: null,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.Elasticsearch.IsTraceMode,
                defaultValue: bool.FalseString,
                isVisibleToClients: false));
    }
}
