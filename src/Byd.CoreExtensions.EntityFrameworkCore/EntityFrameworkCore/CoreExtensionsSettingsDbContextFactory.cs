using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Byd.CoreExtensions.Extensions;
using Volo.Abp.Data;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Ayar yonetimi DbContext'ini tasarim aninda uretir.
/// </summary>
// islevi: dotnet ef komutlari icin ayar context'ini host'un appsettings dosyasindan okunan baglanti dizesiyle kurar.
// sistemdeki gorevi: Ayar tablolari kendi migration setini tasidigi icin bu context'in de kendi tasarim zamani
// fabrikasi vardir; baglanti dizesi context'in kendi ConnectionStringName beyaniyla ayni olmak zorundadir.
public class CoreExtensionsSettingsDbContextFactory : IDesignTimeDbContextFactory<CoreExtensionsSettingsDbContext>
{
    public CoreExtensionsSettingsDbContext CreateDbContext(string[] args)
    {
        // Tasarim aninda host yoktur; model kurulmadan once host'un yaptigi iki hazirlik burada tekrarlanir.
        // Atlanirsa uretilen migration calisan veritabanindan sessizce sapar: sema ve tablo on eki cerceve
        // varsayilanina duser, genisletme kayitlari islenmedigi icin eklenen alanlar kolon olarak dogmaz.
        ServiceConfigurationContextExtension.ResolveSchemaAndPrefix();
        CoreExtensionsEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<CoreExtensionsSettingsDbContext>()
            .UseNpgsql(configuration.GetConnectionString(ConnectionStrings.DefaultConnectionStringName));

        return new CoreExtensionsSettingsDbContext(builder.Options);
    }

    // Dosyalar calisma dizinine gore okunur; dotnet ef komutu startup projesinin dizininde calisir.
    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
