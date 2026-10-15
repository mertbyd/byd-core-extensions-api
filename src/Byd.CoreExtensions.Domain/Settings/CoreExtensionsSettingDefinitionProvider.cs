using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Settings;

/// <summary>
/// Modulun ayarlarini ABP setting management'a tanitir.
/// </summary>
// islevi: Her ayar ailesinin kendi partial metodunu cagirir.
// sistemdeki gorevi: Yeni bir ayar ailesi eklemek bu dosyaya tek satir eklemekten ibarettir; mevcut satirlar degismez. Deger
// burada tutulmaz; varsayilan, appsettings'teki Settings bolumu, global ve tenant degeri arasindan ABP'nin deger saglayici
// zinciri secer.
public partial class CoreExtensionsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        AddElasticsearchSettings(context);
        AddSystemLogSettings(context);
        AddCachingSettings(context);
    }
}
