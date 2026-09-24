using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.EventBus;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.ModuleSettings;
using Byd.CoreExtensions.Providers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions.EventBus;

/// <summary>
/// Tek bir gelen olay yapilandirmasi icin kalici arsivdeki basarisiz olaylari yeniden isler.
/// </summary>
// islevi: Kendi zamanlayicisi uzerinde donup modul ayarina, sahiplik kapsamina ve geri cekilme zamanina uyan kayitlari dagitik kilit altinda yeniden dener.
// sistemdeki gorevi: Cercevenin kisa vadeli vazgecme noktasini modulun uzun ufuklu ve kayit bazinda yalitilmis yeniden deneme dongusune baglar.
public class FailedEventInboxProcessor : IFailedEventInboxProcessor, ITransientDependency
{
    private readonly AbpAsyncTimer _timer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CancellationTokenSource _stoppingTokenSource;
    private InboxConfig? _inboxConfiguration;

    // Zamanlayiciyi ortak tur araligiyla kurar ve her turu RunAsync'e baglar.
    public FailedEventInboxProcessor(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
    {
        _timer = timer;
        _serviceScopeFactory = serviceScopeFactory;
        _timer.Period = (int)TimeSpan
            .FromSeconds(FailedEventInboxConsts.RetryProcessorPeriodSeconds)
            .TotalMilliseconds;
        _timer.Elapsed += _ => RunAsync();
        _stoppingTokenSource = new CancellationTokenSource();
    }

    // Yapilandirmayi baglar ve zamanlayiciyi baslatir.
    public Task StartAsync(InboxConfig inboxConfiguration, CancellationToken cancellationToken = default)
    {
        _inboxConfiguration = inboxConfiguration;
        _timer.Start(cancellationToken);
        return Task.CompletedTask;
    }

    // Devam eden turu iptal eder ve zamanlayiciyi durdurur.
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _stoppingTokenSource.Cancel();
        _timer.Stop(cancellationToken);
        _stoppingTokenSource.Dispose();
        return Task.CompletedTask;
    }

    // Tur basina yeni servis kapsami acar, modul ayarini dogrular ve kuyrugu dagitik kilit altinda bosaltir.
    protected virtual async Task RunAsync()
    {
        if (_inboxConfiguration is null || _stoppingTokenSource.IsCancellationRequested)
        {
            return;
        }
        var stoppingToken = _stoppingTokenSource.Token;
        using var scope = _serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var setting = await services.GetRequiredService<IModuleSettingProvider>()
            .FindCurrentAsync(stoppingToken);
        if (setting is null || !setting.IsEnabled || !setting.EventBoxEnabled)
        {
            return;
        }
        await using var lockHandle = await services.GetRequiredService<IAbpDistributedLock>()
            .TryAcquireAsync(GetDistributedLockName(), cancellationToken: stoppingToken);
        if (lockHandle is null)
        {
            return;
        }
        await DrainRetryableRecordsAsync(
            setting.FailedEventRetryUnlimited,
            setting.FailedEventRetryWindowSeconds,
            services,
            stoppingToken);
    }
    // Kilidi yapilandirma adiyla niteler; her yapilandirma kendi kuyrugunu bagimsiz bosaltir.
    private string GetDistributedLockName()
    {
        return $"{FailedEventInboxConsts.DistributedLockName}_{_inboxConfiguration!.Name}";
    }
    // Kuyruk bosalana ya da tur ustsinirina ulasilana kadar arda arda parti isler.
    // Islenen kayda geri cekilme uygulandigi icin ayni kayit sonraki partide geri gelmez.
    private async Task DrainRetryableRecordsAsync(
        bool retryUnlimited,
        int retryWindowSeconds,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var repository = services.GetRequiredService<IFailedEventInboxRepository>();
        var ownerModuleName = services.GetRequiredService<CurrentModuleNameProvider>().Value;
        var ownerEnvironmentName = services.GetRequiredService<CurrentEnvironmentNameProvider>().Value;
        var clock = services.GetRequiredService<IClock>();
        for (var round = 0; round < FailedEventInboxConsts.MaxRetryRoundsPerRun; round++)
        {
            var records = await repository.GetRetryableListAsync(
                ownerModuleName,
                ownerEnvironmentName,
                clock.Now,
                FailedEventInboxConsts.DefaultRetryBatchSize,
                cancellationToken);
            foreach (var record in records)
            {
                await ProcessRecordAsync(record, retryUnlimited, retryWindowSeconds, services, cancellationToken);
            }
            if (records.Count < FailedEventInboxConsts.DefaultRetryBatchSize)
                return;
        }
    }

    // Tek arsiv kaydini bagimsiz bir transaction icinde isler ve sonucunu kalici kayda uygular.
    private async Task ProcessRecordAsync(
        FailedEventInbox record,
        bool retryUnlimited,
        int retryWindowSeconds,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var manager = services.GetRequiredService<FailedEventInboxManager>();
        if (manager.IsRetryWindowExpired(record, retryUnlimited, retryWindowSeconds))
            return;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var repository = services.GetRequiredService<IFailedEventInboxRepository>();
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        try
        {
            var incomingEvent = new IncomingEventInfo(
                record.Id,
                record.MessageId,
                record.EventName,
                record.EventData,
                record.CreationTime,
                IncomingEventStatus.Pending,
                handledTime: null,
                retryCount: 0,
                nextRetryTime: null);
            await services.GetRequiredService<IDistributedEventBus>()
                .AsSupportsEventBoxes()
                .ProcessFromInboxAsync(incomingEvent, _inboxConfiguration!);
            manager.MarkAsProcessed(record);
        }
        // Handler her tur istisnayi atabilir; tek kaydin donguyu durdurmamasi ve hatanin arsive yazilmasi icin genis yakalama burada bilincli kullanilir.
        catch (Exception exception)
        {
            manager.ScheduleRetry(record, exception.Message, exception.StackTrace);
        }
        await repository.UpdateAsync(record, cancellationToken: cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
