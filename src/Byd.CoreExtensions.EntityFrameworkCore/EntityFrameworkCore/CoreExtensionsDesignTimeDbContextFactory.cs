using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Byd.CoreExtensions.Extensions;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// dotnet ef komutlarinin DbContext uretebilmesi icin ortak tasarim zamani fabrikasi temeli.
/// </summary>
// islevi: Baglanti dizesini host'un ayar dosyasindan okur, PostgreSQL saglayicisini baglar ve modelin
// kurulmasindan once host'un acilista yaptigi hazirligi tekrarlar; context ornegini turemis fabrikaya kurdurur.
// sistemdeki gorevi: Tuketici modullerin turedigi ortak fabrika tabani. BydDbContext nasil ortak DbContext
// davranisini tek yerde tutuyorsa bu tip de tasarim zamani davranisini tek yerde tutar; turemis fabrikaya
// yalnizca "hangi baglanti dizesi" ve "context nasil kurulur" sorulari kalir.
//
// NEDEN BOYLE BIR FABRIKA GEREKIYOR. Saglayici secimi (UseNpgsql) AbpDbContextOptions icinde yasar ve o
// yapilandirma yalnizca ABP uygulamasi ayaga kalkarken islenir. dotnet ef uygulamayi ayaga kaldirmaz;
// assembly'yi yukleyip context'i dogrudan kurmaya calisir. Saglayiciyi kimse vermedigi icin komut
// "No database provider has been configured for this DbContext" ile duser. Bu tip, dotnet ef'in tek
// dayanagi olan IDesignTimeDbContextFactory sozlesmesini karsilar.
//
// NEDEN MODEL ONCESI HAZIRLIK BURADA. Sema/tablo on eki ezmesi ve nesne genisletme kayitlari calisma
// zamaninda host'un acilis asamalarinda yapilir. Tasarim aninda host yoktur; ayni hazirlik burada
// tekrarlanmazsa model cerceve varsayilanlariyla kurulur ve uretilen migration calisan veritabanindan
// sessizce sapar: tablolar yanlis semaya duser, genisletilen alanlar kolon olarak hic dogmaz.
public abstract class CoreExtensionsDesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    // Barindirma ortaminin adini tasiyan degisken; ASP.NET Core'un sozlesmesidir, host ile ayni deger okunur.
    private const string EnvironmentVariableName = "ASPNETCORE_ENVIRONMENT";

    // Bu context'in ayar dosyasinda hangi baglanti dizesini kullandigi; turemis fabrika bildirir.
    // Deger context'in ConnectionStringName beyaniyla ayni olmak zorundadir, aksi halde tasarim zamani
    // ile calisma zamani farkli veritabanlarina bakar.
    protected abstract string ConnectionStringName { get; }

    // Secenekler hazirlandiktan sonra context ornegini uretir; turemis fabrikaya yalnizca yapici cagrisi kalir.
    protected abstract TDbContext CreateInstance(DbContextOptions<TDbContext> options);

    // dotnet ef'in cagirdigi tek uc; args tasarim zamani komutundan gelir ve baglanti secimini etkilemez.
    public TDbContext CreateDbContext(string[] args)
    {
        PrepareModelEnvironment();

        var connectionString = BuildConfiguration().GetConnectionString(ConnectionStringName);
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return CreateInstance(options);
    }

    // Host'un acilista yaptigi, model kurulmadan once tamamlanmasi gereken adimlari tekrarlar.
    // Ikisi de kendi icinde tekrar korumali oldugu icin birden fazla context ureten komutlarda sorun cikarmaz.
    private static void PrepareModelEnvironment()
    {
        ServiceConfigurationContextExtension.ResolveSchemaAndPrefix();
        CoreExtensionsEfCoreEntityExtensionMappings.Configure();
    }

    // Ayar dosyalari calisma klasorunden okunur: dotnet ef, tasarim zamani uygulamasini startup projesinin
    // klasorunde calistirir ve appsettings.json orada durur; bu yuzden ayrica bir taban yol verilmez.
    // Dosya izleme acilmaz, surec tek komutluk omur surer ve yeniden yuklemenin karsiligi yoktur.
    private static IConfiguration BuildConfiguration()
    {
        var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableName);

        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
