using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Byd.CoreExtensions.Extensions;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Core DbContext'ini tasarim aninda uretir.
/// </summary>
// islevi: dotnet ef komutlari icin DbContext'i host'un appsettings dosyasindan okunan baglanti dizesiyle kurar.
// sistemdeki gorevi: Calisma zamaninda saglayici secimini AbpDbContextOptions yapar, ancak o yapilandirma yalnizca
// uygulama ayaga kalkarken calisir; migration uretimi uygulamayi ayaga kaldirmadigi icin saglayici burada verilir.
public class CoreExtensionsDbContextFactory : IDesignTimeDbContextFactory<CoreExtensionsDbContext>
{
    public CoreExtensionsDbContext CreateDbContext(string[] args)
    {
        // Tasarim aninda host yoktur; model kurulmadan once host'un yaptigi iki hazirlik burada tekrarlanir.
        // Atlanirsa uretilen migration calisan veritabanindan sessizce sapar: sema ve tablo on eki cerceve
        // varsayilanina duser, genisletme kayitlari islenmedigi icin eklenen alanlar kolon olarak dogmaz.
        ServiceConfigurationContextExtension.ResolveSchemaAndPrefix();
        CoreExtensionsEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<CoreExtensionsDbContext>()
            .UseNpgsql(configuration.GetConnectionString(CoreExtensionsDbProperties.ConnectionStringName));

        return new CoreExtensionsDbContext(builder.Options);
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
