using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Models.EventBus;
using Byd.CoreExtensions.Providers;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Gelen olay kayitlarini kurulum kapsamiyla birlikte kalici kutuda yonetir.
/// </summary>
// islevi: Kayit sahibi kapsam alanlarini kayda yazar; okuma ve mukerrerlik sorgularini ayni kapsamla sinirlar.
// sistemdeki gorevi: Cercevenin gelen olay store'unu yalnizca modulun kapsam davranislariyla genisleten EF Core kalicilik uygulamasidir.
//
// MIRASTAN GELEN DAVRANIS. Cerceve bekleyen kayitlarda Pending durumunu, next retry zamanini,
// olusma zamanina gore siralamayi ve maxCount sinirini zaten uygular. GetWaitingEventsAsync bu
// sorguyu yeniden yazmaz; yalnizca kayit sahibi modul ve ortam kosullarini filter parametresiyle ekler.
// MarkAsProcessedAsync, RetryLaterAsync ve DeleteOldEventsAsync degistirilmez.
//
// BURADA EKLENEN DAVRANIS. EnqueueAsync kayit sahibi modul ve ortam degerlerini store'un ayni
// calisma ani bagimliliklarindan yazar.
// ExistsByMessageIdAsync yalniz ayni kayit sahibi modul ve ortam icindeki mesaji mukerrer sayar.
// MarkAsDiscardAsync cercevenin vazgectigi kaydi silinmeden once kalici arsive kopyalar.
public class CoreExtensionsEventInbox<TDbContext> : DbContextEventInbox<TDbContext>
    where TDbContext : IHasEventInbox
{
    private readonly CurrentModuleNameProvider _moduleNameProvider;
    private readonly CurrentEnvironmentNameProvider _environmentNameProvider;
    private readonly FailedEventInboxManager _failedEventInboxManager;
    private readonly IFailedEventInboxRepository _failedEventInboxRepository;

    // Cerceve bagimliliklarini tabana devreder ve store kapsam kaynaklarini saklar.
    public CoreExtensionsEventInbox(
        IDbContextProvider<TDbContext> dbContextProvider,
        IClock clock,
        IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions,
        CurrentModuleNameProvider moduleNameProvider,
        CurrentEnvironmentNameProvider environmentNameProvider,
        FailedEventInboxManager failedEventInboxManager,
        IFailedEventInboxRepository failedEventInboxRepository)
        : base(dbContextProvider, clock, eventBusBoxesOptions)
    {
        _moduleNameProvider = moduleNameProvider;
        _environmentNameProvider = environmentNameProvider;
        _failedEventInboxManager = failedEventInboxManager;
        _failedEventInboxRepository = failedEventInboxRepository;
    }

    // Gelen olayi kayit sahibi kapsamiyla isaretleyip cercevenin kalici koleksiyonuna ekler.
    public override async Task EnqueueAsync(IncomingEventInfo incomingEvent)
    {
        var record = new IncomingEventRecord(incomingEvent);
        ApplyOwnerScope(record);
        (await DbContextProvider.GetDbContextAsync()).IncomingEvents.Add(record);
    }

    // Cercevenin bekleme ve siralama sorgusuna yalnizca kurulum kapsami kosulunu ekler.
    public override Task<List<IncomingEventInfo>> GetWaitingEventsAsync(
        int maxCount,
        Expression<Func<IIncomingEventInfo, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var ownerModuleName = _moduleNameProvider.Value;
        var ownerEnvironmentName = _environmentNameProvider.Value;
        Expression<Func<IIncomingEventInfo, bool>> scopeFilter = incomingEvent =>
            EF.Property<string>(incomingEvent, EventBoxConsts.OwnerModuleNamePropertyName) == ownerModuleName &&
            EF.Property<string>(incomingEvent, EventBoxConsts.OwnerEnvironmentNamePropertyName) == ownerEnvironmentName;

        return base.GetWaitingEventsAsync(
            maxCount,
            filter is null ? scopeFilter : scopeFilter.And(filter),
            cancellationToken);
    }

    // Mesaj kimligini yalnizca ayni kayit sahibi modul ve ortam kapsami icinde mukerrer sayar.
    public override async Task<bool> ExistsByMessageIdAsync(string messageId)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();
        var ownerModuleName = _moduleNameProvider.Value;
        var ownerEnvironmentName = _environmentNameProvider.Value;
        return await dbContext.IncomingEvents.AnyAsync(incomingEvent =>
            incomingEvent.MessageId == messageId &&
            EF.Property<string>(incomingEvent, EventBoxConsts.OwnerModuleNamePropertyName) == ownerModuleName &&
            EF.Property<string>(incomingEvent, EventBoxConsts.OwnerEnvironmentNamePropertyName) == ownerEnvironmentName);
    }

    // Cerceve bir olaydan vazgectigi an satiri silmeden once kalici arsive kopyalar.
    // Cerceve vazgecerken istisnayi bu metoda tasimadigi icin hata alanlari ilk yeniden denemeye kadar bos kalir.
    public override async Task MarkAsDiscardAsync(Guid id)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();
        // Kayit izlemeli okunur. Kolona eslenen ek ozellikler ExtraProperties sozluguna ancak
        // degisiklik izleyicisinin materyalizasyon kancasinda dolar; AsNoTracking ile okunan kayitta
        // sahiplik alanlari bos gelir ve arsiv kaydi zorunlu alan dogrulamasindan geceremez.
        var incomingEvent = await dbContext.IncomingEvents
            .FirstOrDefaultAsync(record => record.Id == id);
        if (incomingEvent is not null)
        {
            var model = new FailedEventInboxCreateModel
            {
                MessageId = incomingEvent.MessageId,
                EventName = incomingEvent.EventName,
                EventData = incomingEvent.EventData,
                OwnerModuleName = incomingEvent.GetProperty<string>(EventBoxConsts.OwnerModuleNamePropertyName)!,
                OwnerEnvironmentName = incomingEvent.GetProperty<string>(EventBoxConsts.OwnerEnvironmentNamePropertyName)!,
                ExceptionMessage = null,
                ExceptionStackTrace = null
            };
            await _failedEventInboxRepository.InsertAsync(_failedEventInboxManager.Create(model));
        }

        await base.MarkAsDiscardAsync(id);
    }

    // Kayit sahibi kapsam alanlarini store'un calisma ani kaynaklarindan yazar.
    private void ApplyOwnerScope(IncomingEventRecord record)
    {
        record.SetProperty(EventBoxConsts.OwnerModuleNamePropertyName, _moduleNameProvider.Value);
        record.SetProperty(EventBoxConsts.OwnerEnvironmentNamePropertyName, _environmentNameProvider.Value);
    }


}
