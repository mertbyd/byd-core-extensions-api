using System;

namespace Byd.CoreExtensions.Models.EventBus;

/// <summary>
/// Yeni basarisiz olay kutusu kaydinin domain create girdisini tanimlar.
/// </summary>
// islevi: Kaynak olay kopyasi ile ilk hata bilgisini Manager'in entity kurma akimina tasir.
// sistemdeki gorevi: Sonraki olay isleyicisi ile FailedEventInboxManager arasindaki transporttan bagimsiz girdi sozlesmesidir.
public sealed class FailedEventInboxCreateModel
{
    public string MessageId { get; set; } = default!;
    public string EventName { get; set; } = default!;
    public byte[] EventData { get; set; } = default!;
    public string OwnerModuleName { get; set; } = default!;
    public string OwnerEnvironmentName { get; set; } = default!;
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
}
