using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Byd.CoreExtensions.EntityFrameworkCore;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(CoreExtensionsApplicationModule),
    typeof(CoreExtensionsEntityFrameworkCoreModule),
    typeof(CoreExtensionsHttpApiModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule))]
// islevi: CoreExtensions HTTP hostunun temel ABP modul kompozisyonunu kurar.
// sistemdeki gorevi: Core yetenekleri eklenmeden once API, Swagger ve middleware omurgasini calistirir.
public class CoreExtensionsHttpApiHostModule : AbpModule
{
    // Temel Swagger dokumanini ve cakismasiz schema kimliklerini kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "CoreExtensions API",
                    Version = "v1"
                });
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    // HTTP middleware zincirini API ve Swagger endpointleriyle baslatir.
    public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        if (context.GetEnvironment().IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCorrelationId();
        app.UseRouting();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreExtensions API");
        });
        app.UseConfiguredEndpoints();
        return Task.CompletedTask;
    }
}
