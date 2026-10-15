using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.HealthChecks;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Options;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[DependsOn(
    typeof(CoreExtensionsDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule)
)]
// islevi: Core DbContext'ini, PostgreSQL provider'ini, arama repository'sini ve veri depolarinin saglik yoklamalarini ABP'ye
// kaydeder.
// sistemdeki gorevi: Core entity, repository ve arama repository altyapisinin veri erisimi kompozisyon kokudur. Elasticsearch
// baglantisi ayarlardan gelir; Elasticsearch kapali host'ta arama repository'si istenmedikce hicbir sey degismez.
public class CoreExtensionsEntityFrameworkCoreModule : AbpModule
{
    // Cerceve tiplerine eklenen alanlarin kaydini acilisin en basinda yapar.
    // Bu asama bilerek secildi: kayitlar EF Core modeli kurulurken okunur, dolayisiyla model
    // kurulumundan once tamamlanmis olmalari gerekir. ConfigureServices'e alinirlarsa gec kalir
    // ve eklenen alanlar tabloda kolon olarak dogmaz.
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        CoreExtensionsEfCoreEntityExtensionMappings.Configure();
    }

    // PostgreSQL provider, varsayilan repository, arama repository'si ve saglik yoklamasi kayitlarini tek yerde kurar.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(configurationContext =>
            {
                configurationContext.UseNpgsql();
            });
        });

        // Cercevenin ayar context'i yerine gecen context de burada kaydedilir; kayit olmadan
        // ReplaceDbContext beyani karsiliksiz kalir ve ayar okumasi cercevenin context'ine duser.
        context.Services.AddAbpDbContext<CoreExtensionsSettingsDbContext>();

        context.Services.AddAbpDbContext<CoreExtensionsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);

            /* Add custom repositories here. Example:
             * options.AddRepository<Question, EfCoreQuestionRepository>();
             */
        });

        context.Services.AddCoreExtensionsEventBoxes();

        // Acik generic tip konvansiyonel kayda girmez; her belge tipi bu kayitla dogrudan enjekte edilir.
        context.Services.AddTransient(typeof(IElasticsearchRepository<,>), typeof(ElasticsearchRepository<,>));
        AddHealthChecks(context);
    }

    // Veritabani ve Elasticsearch yoklamalarini host'un sectigi seceneklere gore kaydeder.
    private static void AddHealthChecks(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<CoreExtensionsHealthCheckOptions>();
        if (!options.IsHealthCheckEnabled)
            return;
        var builder = context.Services.AddHealthChecks();
        if (options.IsDatabaseHealthCheckEnabled)
            builder.AddCheck<DatabaseHealthCheck>(HealthCheckConsts.DatabaseName);
        if (options.IsElasticsearchHealthCheckEnabled)
            builder.AddCheck<ElasticsearchHealthCheck>(HealthCheckConsts.ElasticsearchName);
    }
}
