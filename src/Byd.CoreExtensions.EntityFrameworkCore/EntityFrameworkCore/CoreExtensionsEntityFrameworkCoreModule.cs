using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[DependsOn(
    typeof(CoreExtensionsDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule)
)]
// islevi: Core DbContext'ini ve PostgreSQL provider'ini ABP'ye kaydeder.
// sistemdeki gorevi: Core entity ve repository altyapisinin EF Core kompozisyon kokudur.
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

    // PostgreSQL provider ve varsayilan repository kayitlarini tek yerde kurar.
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
    }
}
