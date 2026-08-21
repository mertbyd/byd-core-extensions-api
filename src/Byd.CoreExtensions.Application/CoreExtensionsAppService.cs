using Byd.CoreExtensions.Localization;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions;

public abstract class CoreExtensionsAppService : ApplicationService
{
    protected CoreExtensionsAppService()
    {
        LocalizationResource = typeof(CoreExtensionsResource);
        ObjectMapperContext = typeof(CoreExtensionsApplicationModule);
    }
}
