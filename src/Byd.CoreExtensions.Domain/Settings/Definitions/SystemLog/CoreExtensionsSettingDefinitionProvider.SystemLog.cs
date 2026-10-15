using System.Globalization;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Settings;

// islevi: Sistem log akisi ayarlarini ABP ayar tanimlarina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir.
public partial class CoreExtensionsSettingDefinitionProvider
{
    private static void AddSystemLogSettings(ISettingDefinitionContext context)
    {
        // Log akisi varsayilan olarak kapalidir; Elasticsearch acik olsa bile host loglari ancak bu ayarla gider.
        context.Add(
            new SettingDefinition(
                CoreExtensionsSettings.SystemLog.IsElasticsearchEnabled,
                defaultValue: bool.FalseString,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.SystemLog.IndexName,
                defaultValue: null,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.SystemLog.MinimumLevel,
                defaultValue: SystemLogConsts.DefaultMinimumLevel,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.SystemLog.IsSelfLogEnabled,
                defaultValue: bool.FalseString,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.SystemLog.FailureLogPath,
                defaultValue: null,
                isVisibleToClients: false),
            new SettingDefinition(
                CoreExtensionsSettings.SystemLog.RefreshIntervalSeconds,
                defaultValue: SystemLogConsts.DefaultRefreshIntervalSeconds.ToString(CultureInfo.InvariantCulture),
                isVisibleToClients: false));
    }
}
