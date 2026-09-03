using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions.EntityFrameworkCore;

[DependsOn(
    typeof(CoreExtensionsApplicationTestModule),
    typeof(CoreExtensionsEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class CoreExtensionsEntityFrameworkCoreTestModule : AbpModule
{
    // Paylasilan bellek ici veritabanini hayatta tutan baglantilar; GC toplamasin diye saklanir.
    // Son baglanti kapandiginda SQLite veritabanini yok ettigi icin her test host'u kendi
    // keep-alive baglantisini acik tutar.
    private static readonly List<SqliteConnection> KeepAliveConnections = new();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        var connectionString = CreateDatabaseAndGetConnectionString();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(connectionString);
            });
        });
    }

    // Her test host'u icin ayri adlandirilmis, paylasilan onbellekli bellek ici veritabani kurar.
    // Baglanti nesnesi degil baglanti dizesi verilir: EF her DbContext icin kendi baglantisini acar.
    // Tek bir SqliteConnection ornegi paylasildiginda EF, yeni baglanti sarmalayicisi kurarken
    // CreateFunction/CreateAggregate cagirir ve o an acik bir okuyucu varsa SQLite "database is
    // locked" hatasi verir; ayri baglantilar bu yarisi ortadan kaldirir.
    private static string CreateDatabaseAndGetConnectionString()
    {
        var connectionString =
            $"Data Source=CoreExtensionsTests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        var keepAliveConnection = new SqliteConnection(connectionString);
        keepAliveConnection.Open();
        KeepAliveConnections.Add(keepAliveConnection);

        new CoreExtensionsDbContext(
            new DbContextOptionsBuilder<CoreExtensionsDbContext>()
                .UseSqlite(keepAliveConnection).Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connectionString;
    }
}
