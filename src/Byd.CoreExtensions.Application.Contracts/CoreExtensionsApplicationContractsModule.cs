using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class CoreExtensionsApplicationContractsModule : AbpModule
{

}
