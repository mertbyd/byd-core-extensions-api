using Byd.CoreExtensions.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace Byd.CoreExtensions;

public abstract class CoreExtensionsAppService : ApplicationService
{
    protected CoreExtensionsAppService()
    {
        LocalizationResource = typeof(CoreExtensionsResource);
        ObjectMapperContext = typeof(CoreExtensionsApplicationModule);
    }
    // ABP lazy provider uzerinden repository-native property injection destegini sunar.
    protected TService LazyGetRequiredService<TService>()
        where TService : notnull
    {
        return LazyServiceProvider.LazyGetRequiredService<TService>();
    }
    // Aktif application isteginin iptal belirtecini lazy provider uzerinden acar.
    protected ICancellationTokenProvider CancellationTokenProvider =>
        LazyGetRequiredService<ICancellationTokenProvider>();
}
