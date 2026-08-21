using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsDomainModule),
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
    )]
public class CoreExtensionsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<CoreExtensionsApplicationModule>();
    }
}
