using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Entities.EmailEvents;
using Byd.CoreExtensions.Entities.EmailTemplates;
using Byd.CoreExtensions.Entities.EventBus;
using Byd.CoreExtensions.Entities.ModuleSettings;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[ConnectionStringName(CoreExtensionsDbProperties.ConnectionStringName)]
public interface ICoreExtensionsDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
    DbSet<ModuleSetting> ModuleSettings { get; }

    DbSet<ApiKey> ApiKeys { get; }

    DbSet<FailedEventInbox> FailedEventInboxes { get; }

    DbSet<EmailEvent> EmailEvents { get; }

    DbSet<EmailTemplate> EmailTemplates { get; }
}
