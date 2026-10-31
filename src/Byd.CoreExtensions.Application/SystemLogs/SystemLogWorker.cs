using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Providers;
using Byd.CoreExtensions.Settings;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions.SystemLogs;

/// <summary>
/// Sistem log ayarlarini periyodik okuyup degistiyse host'un logger'ini yeniden kurar.
/// </summary>
// islevi: Her turda ayarlari okur; goruntu oncekinden farkliysa logger'i ortak yapilandirma ve gerekiyorsa Elasticsearch hedefiyle
// yeniden kurar, sonra bir sonraki turun periyodunu ayardan alir.
// sistemdeki gorevi: Elasticsearch'e log gonderimini host yeniden baslatilmadan acip kapatan tek nokta. Host Program.cs'te
// baslangic logger'ini CreateBootstrapLogger ile kurmali; aksi halde ayar uygulanamaz ve uyari yazilir.
// mirastan gelen:
//   ABP AsyncPeriodicBackgroundWorkerBase -> StartAsync/StopAsync ve DoWorkAsync hatalarinin loglanmasi (ayrica try/catch yazilmaz)
// burada yazilan: DoWorkAsync, ReadSnapshotAsync, Apply, ToDataStreamPart, ToPeriodMilliseconds
public class SystemLogWorker : AsyncPeriodicBackgroundWorkerBase
{
    private SystemLogSnapshot? _applied;

    // Ilk tur acilista calisir; ayar okunana kadar varsayilan periyot kullanilir.
    public SystemLogWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = ToPeriodMilliseconds(SystemLogConsts.DefaultRefreshIntervalSeconds);
        Timer.RunOnStart = true;
    }

    // Ayarlari kendi unit of work'unde okur, degistiyse uygular ve periyodu son okunan araliga cevirir.
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var settingProvider = workerContext.ServiceProvider.GetRequiredService<ISettingProvider>();
        using var unitOfWork = unitOfWorkManager.Begin();
        var snapshot = await ReadSnapshotAsync(settingProvider, workerContext.ServiceProvider, workerContext.CancellationToken);
        var intervalSeconds = await settingProvider.GetAsync(
            CoreExtensionsSettings.SystemLog.RefreshIntervalSeconds,
            SystemLogConsts.DefaultRefreshIntervalSeconds);
        await unitOfWork.CompleteAsync(workerContext.CancellationToken);
        if (snapshot != _applied)
        {
            Apply(snapshot);
            _applied = snapshot;
        }
        Timer.Period = ToPeriodMilliseconds(intervalSeconds);
    }

    // Elasticsearch ve sistem log ayarlarini tek goruntude toplar; indeks adi bossa host'un modul adini kullanir.
    private static async Task<SystemLogSnapshot> ReadSnapshotAsync(
        ISettingProvider settingProvider,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var indexName = await settingProvider.GetOrNullAsync(CoreExtensionsSettings.SystemLog.IndexName)
                        ?? serviceProvider.GetRequiredService<CurrentModuleNameProvider>().Value;
        var minimumLevel = await settingProvider.GetOrNullAsync(CoreExtensionsSettings.SystemLog.MinimumLevel);
        return new SystemLogSnapshot(
            await settingProvider.IsTrueAsync(CoreExtensionsSettings.Elasticsearch.IsEnabled)
            && await settingProvider.IsTrueAsync(CoreExtensionsSettings.SystemLog.IsElasticsearchEnabled),
            await settingProvider.GetOrNullAsync(CoreExtensionsSettings.Elasticsearch.Url),
            await settingProvider.GetOrNullAsync(CoreExtensionsSettings.Elasticsearch.UserName),
            await settingProvider.GetOrNullAsync(CoreExtensionsSettings.Elasticsearch.Password),
            ToDataStreamPart(indexName),
            ToDataStreamPart(serviceProvider.GetRequiredService<CurrentEnvironmentNameProvider>().Value),
            Enum.TryParse<LogEventLevel>(minimumLevel, ignoreCase: true, out var level) ? level : LogEventLevel.Information,
            await settingProvider.IsTrueAsync(CoreExtensionsSettings.SystemLog.IsSelfLogEnabled),
            await settingProvider.GetOrNullAsync(CoreExtensionsSettings.SystemLog.FailureLogPath));
    }

    // Akis tanilarini acip kapatir ve baslangic logger'ini goruntuye gore yeniden kurar.
    private void Apply(SystemLogSnapshot snapshot)
    {
        if (snapshot.IsSelfLogEnabled)
            SelfLog.Enable(Console.Error);
        else
            SelfLog.Disable();
        if (Log.Logger is not ReloadableLogger logger)
        {
            Logger.LogWarning("System log settings were not applied; the host must create its logger with CreateBootstrapLogger.");
            return;
        }
        logger.Reload(configuration => configuration
            .ConfigureSystemLog(snapshot.EnvironmentName)
            .WriteToElasticsearch(snapshot));
    }

    // Veri akisi adinin bir parcasini Elasticsearch kuralina uydurur: kucuk harf ve ayirici yerine nokta.
    private static string ToDataStreamPart(string value)
    {
        return value.ToLowerInvariant().Replace(SystemLogConsts.DataStreamSeparator, SystemLogConsts.DataStreamSeparatorReplacement);
    }

    // Saniye cinsinden araligi timer'in bekledigi milisaniye periyoduna cevirir.
    private static int ToPeriodMilliseconds(int seconds)
    {
        return (int)TimeSpan.FromSeconds(seconds).TotalMilliseconds;
    }
}
