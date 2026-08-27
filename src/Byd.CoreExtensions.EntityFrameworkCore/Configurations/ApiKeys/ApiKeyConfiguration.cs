using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.CoreExtensions.EntityFrameworkCore;

// islevi: ApiKey entity'sinin tablo adini, kolon kisitlarini ve anahtar deger uzerindeki benzersiz indeksi map eder.
// sistemdeki gorevi: API anahtari tablosunun PostgreSQL ve SQLite semasindaki tek EF konfigurasyon sahibidir.
public sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    // ABP convention alanlarindan sonra ApiKey kolon ve indeks sozlesmesini uygular.
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable(
            CoreExtensionsDbProperties.DbTablePrefix + ApiKeyConsts.TableName,
            CoreExtensionsDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.Key).IsRequired().HasMaxLength(ApiKeyConsts.MaxKeyLength);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(ApiKeyConsts.MaxNameLength);
        builder.HasIndex(entity => entity.Key).IsUnique();
    }
}
