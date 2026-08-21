using Localization.Resources.AbpUi;
using Byd.CoreExtensions.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class CoreExtensionsHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(CoreExtensionsHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<CoreExtensionsResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
