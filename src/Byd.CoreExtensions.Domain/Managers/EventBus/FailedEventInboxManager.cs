using System;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.EventBus;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.EventBus;

namespace Byd.CoreExtensions.Managers;

/// <summary>
/// Basarisiz olay kaydi olusturma, yeniden deneme ve tamamlanma kararlarini yonetir.
/// </summary>
// islevi: Metin alanlarini kanoniklestirir, ilk yeniden deneme durumunu kurar ve data-shell entity mutasyonlarini yapar.
// sistemdeki gorevi: Sonraki isleyicileri is kararindan ve entity'yi davranistan uzak tutan domain sahibidir.
public class FailedEventInboxManager : BaseManager<FailedEventInbox, Guid>
{
    // Basarisiz olay extended repository'sini ortak manager tabanina devreder.
    public FailedEventInboxManager(IFailedEventInboxRepository repository)
        : base(repository)
    {
    }

    // Dogrulanmis create modelinden hemen yeniden denenebilir basarisiz olay kaydini kurar.
    public virtual FailedEventInbox Create(FailedEventInboxCreateModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new FailedEventInbox(GuidGenerator.Create())
        {
            MessageId = NormalizeRequiredText(model.MessageId),
            EventName = NormalizeRequiredText(model.EventName),
            EventData = model.EventData,
            OwnerModuleName = NormalizeRequiredText(model.OwnerModuleName),
            OwnerEnvironmentName = NormalizeRequiredText(model.OwnerEnvironmentName),
            IsProcessed = false,
            NotBefore = Clock.Now,
            RetryCount = 0,
            ExceptionMessage = NormalizeOptionalText(model.ExceptionMessage),
            ExceptionStackTrace = NormalizeOptionalText(model.ExceptionStackTrace)
        };
    }

    // Yeniden denemesi basaran kaydi islenmis olarak damgalar.
    public virtual void MarkAsProcessed(FailedEventInbox entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.IsProcessed = true;
        entity.ProcessedTime = Clock.Now;
    }

    // Denemeyi bir sonraki pencereye oteler ve son hatayi kaydeder.
    public virtual void ScheduleRetry(
        FailedEventInbox entity,
        string? exceptionMessage,
        string? exceptionStackTrace)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.RetryCount = entity.RetryCount == int.MaxValue
            ? int.MaxValue
            : entity.RetryCount + 1;
        var delaySeconds = Math.Min(
            FailedEventInboxConsts.RetryBackoffFactorSeconds * Math.Pow(2, entity.RetryCount),
            FailedEventInboxConsts.MaxRetryBackoffSeconds);
        entity.NotBefore = Clock.Now.AddSeconds(delaySeconds);
        entity.ExceptionMessage = NormalizeOptionalText(exceptionMessage);
        entity.ExceptionStackTrace = NormalizeOptionalText(exceptionStackTrace);
    }

    // Modul ayarindaki sure sinirina gore kaydin yeniden deneme penceresinin dolup dolmadigini bildirir.
    public virtual bool IsRetryWindowExpired(
        FailedEventInbox entity,
        bool retryUnlimited,
        int retryWindowSeconds)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return !retryUnlimited && Clock.Now >= entity.CreationTime.AddSeconds(retryWindowSeconds);
    }
}
