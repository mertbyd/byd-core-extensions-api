using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class CoreExtensionsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(CoreExtensionsApplicationContractsModule).Assembly,
            CoreExtensionsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CoreExtensionsHttpApiClientModule>();
        });

    }
}
