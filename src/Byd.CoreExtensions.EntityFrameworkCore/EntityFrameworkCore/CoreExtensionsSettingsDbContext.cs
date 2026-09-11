using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// ABP ayar yonetimi tablolarini modulun adlandirma bicimiyle sahiplenen DbContext.
/// </summary>
// islevi: Ayar tablolarinin modelini kurar ve fiziksel adlarini snake_case bicimine cevirir.
// sistemdeki gorevi: ReplaceDbContext beyani sayesinde cercevenin SettingManagementDbContext'i yerine gecer.
// Ayni tabloyu iki context'in farkli adlandirmayla sahiplenmesini engelleyen tek yerdir: model seviyesindeki
// cevirim yalniz sahibi oldugumuz OnModelCreating'den uygulanabildigi icin, tuketen host'lar aksi halde
// ayar okumasini kendi tarafinda ayrica adlandirma ayariyla duzeltmek zorunda kalir.
[ReplaceDbContext(typeof(ISettingManagementDbContext))]
[ConnectionStringName(ConnectionStrings.DefaultConnectionStringName)]
public class CoreExtensionsSettingsDbContext : AbpDbContext<CoreExtensionsSettingsDbContext>, ISettingManagementDbContext
{
    // Ayar entity'leri cerceveden gelir; ISettingManagementDbContext bu iki seti talep eder.
    public DbSet<Setting> Settings { get; set; } = default!;

    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; } = default!;

    public CoreExtensionsSettingsDbContext(DbContextOptions<CoreExtensionsSettingsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSettingManagement();
        builder.ApplySnakeCaseNaming();
    }
}
