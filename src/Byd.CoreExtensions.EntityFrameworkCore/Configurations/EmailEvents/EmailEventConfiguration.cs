using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.EmailEvents;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// EmailEvent lookup tablosunun kolon ve ad benzersizligi sozlesmesini tanimlar.
/// </summary>
// islevi: E-posta olayi entity'sini kararli tablo adina, ad sinirina ve tekil ad indeksine map eder.
// sistemdeki gorevi: PostgreSQL ve SQLite modellerinin ayni EmailEvent semasini kullanmasini saglar.
public sealed class EmailEventConfiguration : IEntityTypeConfiguration<EmailEvent>
{
    // ABP convention alanlarindan sonra EmailEvent kolon ve indeks sozlesmesini uygular.
    public void Configure(EntityTypeBuilder<EmailEvent> builder)
    {
        builder.ToTable(
            CoreExtensionsDbProperties.DbTablePrefix + EmailEventConsts.TableName,
            CoreExtensionsDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.Name).HasMaxLength(EmailEventConsts.MaxNameLength);
        // Ad, gonderimde olayin bulundugu anahtardir; Manager ayni kurali is hatasi olarak verir, bu indeks son savunmadir.
        builder.HasIndex(entity => entity.Name)
            .IsUnique()
            .HasDatabaseName(EmailEventConsts.NameIndex);
    }
}
