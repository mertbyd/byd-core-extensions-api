using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Ayar yonetimi DbContext'ini tasarim aninda ureten fabrika.
/// </summary>
// islevi: Ayar context'inin baglanti dizesi adini bildirir ve ornegini kurar.
// sistemdeki gorevi: Ayar tablolari kendi migration setini tasidigi icin bu context'in de ayri bir
// tasarim zamani girisi vardir; baglanti dizesi context'in kendi ConnectionStringName beyaniyla aynidir.
public class CoreExtensionsSettingsDbContextFactory
    : CoreExtensionsDesignTimeDbContextFactory<CoreExtensionsSettingsDbContext>
{
    protected override string ConnectionStringName => ConnectionStrings.DefaultConnectionStringName;

    protected override CoreExtensionsSettingsDbContext CreateInstance(
        DbContextOptions<CoreExtensionsSettingsDbContext> options)
        => new(options);
}
