using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[ConnectionStringName(CoreExtensionsDbProperties.ConnectionStringName)]
public interface ICoreExtensionsDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
