using System.Globalization;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Settings;

/// <summary>
/// Rate limit global varsayilan ayarlarini ABP setting sistemine tanimlar.
/// </summary>
// islevi: Dinamik rate limit varsayilanlarini (algoritma, istek limiti, pencere) ABP ayar tanimi olarak kaydeder.
// sistemdeki gorevi: Dinamik rate limit yapisinin ayar kaynagi; Host cozucu bu ayarlari ISettingProvider ile okuyup bellek snapshot'ina aktarir.
public class CoreExtensionsSettingDefinitionProvider : SettingDefinitionProvider
{
    // Rate limit global varsayilan ayarlarini sabit varsayilan degerleriyle tanimlar.
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                CoreExtensionsSettings.RateLimitDefaultAlgorithm,
                RateLimitConsts.DefaultAlgorithm),
            new SettingDefinition(
                CoreExtensionsSettings.RateLimitDefaultPermitLimit,
                RateLimitConsts.DefaultPermitLimit.ToString(CultureInfo.InvariantCulture)),
            new SettingDefinition(
                CoreExtensionsSettings.RateLimitDefaultWindowSeconds,
                RateLimitConsts.DefaultWindowSeconds.ToString(CultureInfo.InvariantCulture)));
    }
}
