using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Byd.CoreExtensions.EntityFrameworkCore;

public static class CoreExtensionsDbContextModelCreatingExtensions
{
    public static void ConfigureCoreExtensions(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // Bu assembly'deki tum IEntityTypeConfiguration siniflarini tarayip uygular;
        // yeni bir konfigurasyon eklendiginde burada elle kayit gerekmez.
        builder.ApplyConfigurationsFromAssembly(
            typeof(CoreExtensionsDbContextModelCreatingExtensions).Assembly);

        /* Configure all entities here. Example:

        builder.Entity<Question>(b =>
        {
            //Configure table & schema name
            b.ToTable(CoreExtensionsDbProperties.DbTablePrefix + "Questions", CoreExtensionsDbProperties.DbSchema);

            b.ConfigureByConvention();

            //Properties
            b.Property(q => q.Title).IsRequired().HasMaxLength(QuestionConsts.MaxTitleLength);

            //Relations
            b.HasMany(question => question.Tags).WithOne().HasForeignKey(qt => qt.QuestionId);

            //Indexes
            b.HasIndex(q => q.CreationTime);
        });
        */
    }
}
