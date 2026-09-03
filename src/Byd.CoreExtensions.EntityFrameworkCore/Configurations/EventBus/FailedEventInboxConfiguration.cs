using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.EventBus;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Basarisiz olay kutusu tablosunun kolon ve indeks sozlesmesini tanimlar.
/// </summary>
// islevi: Basarisiz olay entity'sini on eksiz tablo adina, alan sinirlarina ve yeniden deneme indekslerine map eder.
// sistemdeki gorevi: PostgreSQL ve SQLite modellerinin ayni kalici olay kopyasi semasini kullanmasini saglar.
public sealed class FailedEventInboxConfiguration : IEntityTypeConfiguration<FailedEventInbox>
{
    // ABP convention alanlarindan sonra basarisiz olay kolon ve indeks sozlesmesini uygular.
    public void Configure(EntityTypeBuilder<FailedEventInbox> builder)
    {
        // Bu tablo ayni ailedeki olay kutulari gibi bilerek modul on eki kullanmaz.
        builder.ToTable(
            FailedEventInboxConsts.TableName,
            CoreExtensionsDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.MessageId)
            .IsRequired()
            .HasMaxLength(FailedEventInboxConsts.MaxMessageIdLength);
        builder.Property(entity => entity.EventName)
            .IsRequired()
            .HasMaxLength(FailedEventInboxConsts.MaxEventNameLength);
        builder.Property(entity => entity.EventData)
            .IsRequired();
        builder.Property(entity => entity.OwnerModuleName)
            .IsRequired()
            .HasMaxLength(EventBoxConsts.MaxOwnerModuleNameLength);
        builder.Property(entity => entity.OwnerEnvironmentName)
            .IsRequired()
            .HasMaxLength(EventBoxConsts.MaxOwnerEnvironmentNameLength);
        builder.Property(entity => entity.ExceptionMessage)
            .HasMaxLength(FailedEventInboxConsts.MaxExceptionMessageLength);
        builder.HasIndex(entity => new
            {
                entity.OwnerModuleName,
                entity.OwnerEnvironmentName,
                entity.IsProcessed,
                entity.NotBefore
            })
            .HasDatabaseName(FailedEventInboxConsts.RetryScopeIndex);
        builder.HasIndex(entity => entity.MessageId)
            .HasDatabaseName(FailedEventInboxConsts.MessageIdIndex);
    }
}
