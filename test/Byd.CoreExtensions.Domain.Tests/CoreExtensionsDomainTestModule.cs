using Volo.Abp.Modularity;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsDomainModule),
    typeof(CoreExtensionsTestBaseModule)
)]
public class CoreExtensionsDomainTestModule : AbpModule
{

}
