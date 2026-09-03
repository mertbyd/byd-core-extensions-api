using Byd.CoreExtensions.Options;
using Volo.Abp.Modularity;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsApplicationModule),
    typeof(CoreExtensionsDomainTestModule)
    )]
public class CoreExtensionsApplicationTestModule : AbpModule
{
    // Test host'u da gercek host gibi modul ve ortam adini beyan eder; beyan olmadan modul ayaga kalkmaz.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<CoreExtensionsModuleOptions>(options =>
        {
            options.ModuleName = "CoreExtensions";
            options.EnvironmentName = "Test";
        });
    }
}
