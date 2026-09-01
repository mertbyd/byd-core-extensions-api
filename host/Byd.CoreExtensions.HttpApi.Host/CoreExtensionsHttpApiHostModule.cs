using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Byd.CoreExtensions.Options;
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

    // Modul beyanini, temel Swagger dokumanini, cakismasiz schema kimliklerini ve XML aciklamalarini kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Bu host'un ModuleSettings satir adi ve anti-duplicate muafiyetleri; beyan zorunludur.
        // Satir adi giris assembly'sinin adindan host son eki atilarak turetilir.
        Configure<CoreExtensionsModuleOptions>(options =>
        {
            options.ModuleName = Assembly.GetEntryAssembly()?.GetName().Name?
                .Replace(".HttpApi.Host", string.Empty);
        });
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
        // Modulu bu pipeline'a bagladigimizi beyan eder ve mukerrer istek korumasini buraya takar;
        // cagrilmazsa modul acilista durur. Rate limit'in altinda durur: limiti asan istek govdesi
        // hic okunmadan reddedilsin diye.
        app.UseCoreExtensions();
        app.UseConfiguredEndpoints();
        return Task.CompletedTask;
    }
}
