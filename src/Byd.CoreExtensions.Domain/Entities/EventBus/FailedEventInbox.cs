using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.CoreExtensions.Entities.EventBus;

/// <summary>
/// Cerceve yeniden denemelerinden sonra basarisiz kalan olay kaydini kalici olarak tasir.
/// </summary>
// islevi: Kaynak gelen olay silinse bile olay govdesini, kapsamini, hata bilgisini ve yeniden deneme durumunu saklar.
// sistemdeki gorevi: Uzun ufuklu yeniden deneme ve operasyonel okuma icin kurallari Manager'a birakan veri kabugudur.
//
// KAYNAK KOPYA BILINCLI BIR KARARDIR. Cerceve vazgectigi gelen olay kaydini kalici olarak
// silebildigi icin EventName, EventData, OwnerModuleName ve OwnerEnvironmentName bu tabloda
// yeniden saklanir. Boylece kaynak satir gittikten sonra da kayit tek basina anlamli kalir.
//
// Priority bilerek alinmadi; hicbir sorgu veya karar tarafindan okunmuyor. FailStatus da
// IsProcessed ile RetryCount'un tasidigi bilgiyi tekrar edecegi icin ayrica saklanmiyor.
public class FailedEventInbox : AuditedAggregateRoot<Guid>
{
    public string MessageId { get; internal set; } = default!;
    public string EventName { get; internal set; } = default!;
    public string OwnerModuleName { get; internal set; } = default!;
    public string OwnerEnvironmentName { get; internal set; } = default!;
    public bool IsProcessed { get; internal set; }
    public DateTime? ProcessedTime { get; internal set; }
    public DateTime NotBefore { get; internal set; }
    public int RetryCount { get; internal set; }
    public string? ExceptionMessage { get; internal set; }
    public string? ExceptionStackTrace { get; internal set; }
    public byte[] EventData { get; internal set; } = default!;

    // EF Core materializasyonu icin parametresiz ctor.
    protected FailedEventInbox()
    {
    }

    // Manager tarafindan uretilen kimlikle bos veri kabugunu kurar.
    internal FailedEventInbox(Guid id)
        : base(id)
    {
    }
}
