using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[ConnectionStringName(CoreExtensionsDbProperties.ConnectionStringName)]
public class CoreExtensionsDbContext : AbpDbContext<CoreExtensionsDbContext>, ICoreExtensionsDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public CoreExtensionsDbContext(DbContextOptions<CoreExtensionsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSettingManagement();

        builder.ConfigureCoreExtensions();
    }
}
