using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;

namespace Byd.CoreExtensions.EntityFrameworkCore;

public static class CoreExtensionsDbContextModelCreatingExtensions
{
    public static void ConfigureCoreExtensions(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // Kutu tablolarinin kolon kisitlarini ve indekslerini cerceve kurar: olay adi uzunlugu,
        // olay govdesinin zorunlulugu, gelen tarafta (durum, olusma zamani) ve mesaj kimligi
        // indeksleri, giden tarafta olusma zamani indeksi. Bunlari elle tekrar yazmiyoruz.
        //
        // Cercevenin tek eksigi tablonun nereye dusecegi: kendi on ekini ve kendi semasini
        // kullanir, modul semamiza gelmez. Bu yuzden asagidaki tarama ile gelen
        // EventInboxConfiguration ve EventOutboxConfiguration yalnizca tablo adini ve semayi ezer.
        //
        // SIRA ONEMLI. Once cercevenin kurulumu, sonra bizim konfigurasyonlarimiz calismali;
        // tablo adini son yazan kazanir. Bu iki cagriyi asagidaki taramanin altina alirsan
        // tablolar modul semasindan cikar ve cercevenin varsayilan semasina duser.
        builder.ConfigureEventInbox();
        builder.ConfigureEventOutbox();

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
