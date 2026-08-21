using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using Byd.CoreExtensions.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpDddDomainSharedModule)
)]
public class CoreExtensionsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CoreExtensionsDomainSharedModule>();
        });
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<CoreExtensionsResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson(CoreExtensionsLocalizationConsts.VirtualPath);
            // Postgres saglayici hatalari genel kaynaktan ayri tutulur; kendi sanal klasorunden beslenir.
            options.Resources
                .Add<CoreExtensionsPostgresExceptionsResource>("en")
                .AddVirtualJson(CoreExtensionsLocalizationConsts.PostgresExceptions.VirtualPath);
        });
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("CoreExtensions", typeof(CoreExtensionsResource));
        });
    }
}
