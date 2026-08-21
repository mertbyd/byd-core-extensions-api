using System;
using Microsoft.Extensions.Localization;
using Byd.CoreExtensions.Localization;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.Providers;

/// <summary>
/// Enum degerlerinin lokalize aciklamalarini modulun genel kaynagindan cozen saglayici.
/// </summary>
// islevi: Bir enum degerini kararli bir anahtara cevirip modul kaynagindan lokalize metnini okur.
// sistemdeki gorevi: Enum aciklamalarinin her cagri yerinde elle anahtar kurulmasini engeller; anahtar bicimi LocalizationConsts'ta tek sahipte durur.
public class GlobalLocalizationProvider : ITransientDependency
{
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;

    public GlobalLocalizationProvider(IStringLocalizer<CoreExtensionsResource> localizer)
    {
        _localizer = localizer;
    }

    // Enum degerini kararli anahtara cevirip lokalize aciklamasini dondurur.
    public virtual string GetDescription(Enum value)
    {
        var key = string.Format(
            CoreExtensionsLocalizationConsts.EnumKeyFormat,
            value.GetType().Name,
            value);

        return _localizer[key];
    }
}
