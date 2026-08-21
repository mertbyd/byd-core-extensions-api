using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(CoreExtensionsDomainSharedModule)
)]
public class CoreExtensionsDomainModule : AbpModule
{

}
