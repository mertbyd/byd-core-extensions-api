using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using Volo.Abp.FluentValidation;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpFluentValidationModule)
    )]
public class CoreExtensionsApplicationContractsModule : AbpModule
{

}
