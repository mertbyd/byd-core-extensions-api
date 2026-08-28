using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Entities.RateLimit;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[ConnectionStringName(CoreExtensionsDbProperties.ConnectionStringName)]
public interface ICoreExtensionsDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
    DbSet<ModuleSetting> ModuleSettings { get; }

    DbSet<ApiKey> ApiKeys { get; }
}
