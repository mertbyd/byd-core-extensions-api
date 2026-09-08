using Microsoft.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Core DbContext'ini tasarim aninda ureten fabrika.
/// </summary>
// islevi: Core context'inin baglanti dizesi adini bildirir ve ornegini kurar; kalan her sey tabanda yasar.
// sistemdeki gorevi: Modulun kendi migration setini uretirken dotnet ef'in kullandigi giris noktasidir.
public class CoreExtensionsDbContextFactory
    : CoreExtensionsDesignTimeDbContextFactory<CoreExtensionsDbContext>
{
    protected override string ConnectionStringName => CoreExtensionsDbProperties.ConnectionStringName;

    protected override CoreExtensionsDbContext CreateInstance(
        DbContextOptions<CoreExtensionsDbContext> options)
        => new(options);
}
