using Byd.CoreExtensions.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Byd.CoreExtensions;

public abstract class CoreExtensionsController : AbpControllerBase
{
    protected CoreExtensionsController()
    {
        LocalizationResource = typeof(CoreExtensionsResource);
    }
}
