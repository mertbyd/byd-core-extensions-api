using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(CoreExtensionsDomainSharedModule),
    typeof(AbpSettingManagementDomainModule)
)]
public class CoreExtensionsDomainModule : AbpModule
{

}
