using Byd.CoreExtensions.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions;

public abstract class CoreExtensionsController : AbpControllerBase
{
    protected CoreExtensionsController()
    {
        LocalizationResource = typeof(CoreExtensionsResource);
    }

    // ABP lazy provider uzerinden controller servislerini repository-native sekilde cozer.
    protected TService LazyGetRequiredService<TService>()
        where TService : notnull
    {
        return LazyServiceProvider.LazyGetRequiredService<TService>();
    }
}
