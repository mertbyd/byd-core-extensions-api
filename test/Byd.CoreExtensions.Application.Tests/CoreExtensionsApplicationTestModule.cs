using Volo.Abp.Modularity;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsApplicationModule),
    typeof(CoreExtensionsDomainTestModule)
    )]
public class CoreExtensionsApplicationTestModule : AbpModule
{

}
