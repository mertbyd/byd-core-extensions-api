using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Byd.CoreExtensions.Entities.EventBus;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Extensions;

namespace Byd.CoreExtensions.EntityFrameworkCore;

// IHasEventInbox ve IHasEventOutbox cerceveden gelir ve tek bir sey talep eder: kutu kayitlarini
// tutan iki DbSet. Bu iki arayuzu uygulamak, kutu altyapisinin bu DbContext uzerinden calisacagini
// beyan etmektir; kutuyu okuyup yazan cerceve bilesenleri DbContext'i bu arayuzler uzerinden bulur.
// Asagidaki IncomingEvents ve OutgoingEvents bu sozlesmenin karsiligidir, bizim tercihimiz degil.
[ConnectionStringName(CoreExtensionsDbProperties.ConnectionStringName)]
public class CoreExtensionsDbContext : AbpDbContext<CoreExtensionsDbContext>, ICoreExtensionsDbContext,
    IHasEventInbox, IHasEventOutbox
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public DbSet<ApiKey> ApiKeys { get; set; } = default!;

    public DbSet<ModuleSetting> ModuleSettings { get; set; } = default!;

    public DbSet<FailedEventInbox> FailedEventInboxes { get; set; } = default!;

    // Kutu kayitlarinin entity'leri cerceveden gelir, bu depoda tanimli degildir.
    // Tablo adi ve semasi Configurations/EventBus altindaki konfigurasyonlarda belirlenir.
    public DbSet<IncomingEventRecord> IncomingEvents { get; set; } = default!;

    public DbSet<OutgoingEventRecord> OutgoingEvents { get; set; } = default!;

    public CoreExtensionsDbContext(DbContextOptions<CoreExtensionsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSettingManagement();
        builder.ConfigureCoreExtensions();
        builder.ApplySnakeCaseNaming();
    }
}
