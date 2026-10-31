using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.EmailTemplates;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// EmailTemplate tablosunun kolon ve kapsam indeksi sozlesmesini tanimlar.
/// </summary>
// islevi: E-posta sablonu entity'sini kararli tablo adina, alan sinirlarina ve gonderim sorgusunun indeksine map eder.
// sistemdeki gorevi: PostgreSQL ve SQLite modellerinin ayni EmailTemplate semasini kullanmasini saglar.
public sealed class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    // ABP convention alanlarindan sonra EmailTemplate kolon ve indeks sozlesmesini uygular.
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable(
            CoreExtensionsDbProperties.DbTablePrefix + EmailTemplateConsts.TableName,
            CoreExtensionsDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.Name).HasMaxLength(EmailTemplateConsts.MaxNameLength);
        builder.Property(entity => entity.CultureName).HasMaxLength(EmailTemplateConsts.MaxCultureNameLength);
        builder.Property(entity => entity.Subject).HasMaxLength(EmailTemplateConsts.MaxSubjectLength);
        // Olay silinirse ona bagli sablonlar sessizce kaybolmaz; silme engellenir.
        builder.HasOne(entity => entity.EmailEvent)
            .WithMany()
            .HasForeignKey(entity => entity.EmailEventId)
            .OnDelete(DeleteBehavior.Restrict);
        // Gonderim sorgusu olay ve dili sabitleyip kapsam zincirini tarar; indeks bu sirayla kurulur.
        builder.HasIndex(entity => new { entity.EmailEventId, entity.CultureName, entity.TenantId, entity.OrganizationId })
            .HasDatabaseName(EmailTemplateConsts.ScopeIndex);
    }
}
