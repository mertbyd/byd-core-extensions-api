using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// ModuleSetting tablosunun kolon ve benzersiz indeks sozlesmesini tanimlar.
/// </summary>
// islevi: Modul ayari entity'sini kararli tablo adina, alan sinirlarina ve varsayilanlara map eder.
// sistemdeki gorevi: PostgreSQL ve SQLite modellerinin ayni ModuleSetting semasini kullanmasini saglar.
public sealed class ModuleSettingConfiguration : IEntityTypeConfiguration<ModuleSetting>
{
    // ABP convention alanlarindan sonra ModuleSetting kolon ve indeks sozlesmesini uygular.
    public void Configure(EntityTypeBuilder<ModuleSetting> builder)
    {
        builder.ToTable(
            CoreExtensionsDbProperties.DbTablePrefix + ModuleSettingConsts.TableName,
            CoreExtensionsDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.ModuleName)
            .IsRequired()
            .HasMaxLength(ModuleSettingConsts.MaxModuleNameLength);
        builder.Property(entity => entity.TtlSeconds)
            .HasDefaultValue(ModuleSettingConsts.DefaultTtlSeconds);
        builder.Property(entity => entity.RateLimitPermitLimit)
            .HasDefaultValue(ModuleSettingConsts.DefaultRateLimitPermitLimit);
        builder.Property(entity => entity.RateLimitWindowSeconds)
            .HasDefaultValue(ModuleSettingConsts.DefaultRateLimitWindowSeconds);
        builder.HasIndex(entity => entity.ModuleName)
            .IsUnique()
            .HasDatabaseName(ModuleSettingConsts.ModuleNameIndex);
    }
}
