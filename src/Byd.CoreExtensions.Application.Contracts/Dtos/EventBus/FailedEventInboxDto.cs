using System;

namespace Byd.CoreExtensions.Dtos.EventBus;

/// <summary>
/// Kalici basarisiz olay kaydinin operasyonel cevap alanlarini tasir.
/// </summary>
// islevi: Olay kimligini, kapsamini, isleme durumunu ve son hata bilgisini HTTP ciktisina acar.
// sistemdeki gorevi: HTTP ciktisinda entity, EF tipi ve ikili olay govdesini disariya sizdirmayan response sozlesmesidir.
public class FailedEventInboxDto
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = default!;
    public string EventName { get; set; } = default!;
    public string OwnerModuleName { get; set; } = default!;
    public string OwnerEnvironmentName { get; set; } = default!;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedTime { get; set; }
    public DateTime NotBefore { get; set; }
    public int RetryCount { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
