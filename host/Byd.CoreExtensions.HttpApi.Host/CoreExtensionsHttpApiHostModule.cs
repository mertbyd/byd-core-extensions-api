using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
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
    // ABP tablolarinin sema ve on ekini EF modeli kurulmadan once cozer.
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ServiceConfigurationContextExtension.ResolveSchemaAndPrefix();
    }

    // Temel Swagger dokumanini, cakismasiz schema kimliklerini ve XML aciklamalarini kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc(
                SwaggerConsts.DocumentName,
                new OpenApiInfo
                {
                    Title = SwaggerConsts.DocumentTitle,
                    Version = SwaggerConsts.DocumentName
                });
            options.CustomSchemaIds(type => type.FullName);
            IncludeXmlDocumentation(options);
        });
    }

    // Controller ve DTO XML yorumlarini Swagger dokumanina tasir; dosya uretilmediyse sessizce atlanir.
    private static void IncludeXmlDocumentation(SwaggerGenOptions options)
    {
        var fileNames = new[]
        {
            SwaggerConsts.HttpApiXmlDocumentationFileName,
            SwaggerConsts.ApplicationContractsXmlDocumentationFileName
        };

        foreach (var fileName in fileNames)
        {
            var path = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(path))
            {
                options.IncludeXmlComments(path, includeControllerXmlComments: true);
            }
        }
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
            options.SwaggerEndpoint(SwaggerConsts.DocumentUrl, SwaggerConsts.DocumentTitle);
        });
        app.UseRateLimiter();
        app.UseConfiguredEndpoints();
        return Task.CompletedTask;
    }
}
