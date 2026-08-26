using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
// islevi: Core DbContext'ini ve PostgreSQL provider'ini ABP'ye kaydeder.
// sistemdeki gorevi: Core entity ve repository altyapisinin EF Core kompozisyon kokudur.
public class CoreExtensionsEntityFrameworkCoreModule : AbpModule
{
    // PostgreSQL provider ve varsayilan repository kayitlarini tek yerde kurar.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(configurationContext =>
            {
                configurationContext.UseNpgsql();
                configurationContext.DbContextOptions.UseSnakeCaseNamingConvention();
            });
        });

        context.Services.AddAbpDbContext<CoreExtensionsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);

            /* Add custom repositories here. Example:
             * options.AddRepository<Question, EfCoreQuestionRepository>();
             */
        });
    }
}
