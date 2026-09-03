namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Basarisiz olay kutusu HTTP rotalarini tanimlar.
/// </summary>
// islevi: Basarisiz olay okuma endpointlerinin kararli rota sozlesmesini tek yerde tutar.
// sistemdeki gorevi: Controller ile istemcilerin ayni derleme guvenli route degerlerini kullanmasini saglar.
public static class FailedEventInboxRoutes
{
    public const string Root = "api/core-extensions/failed-event-inboxes";
    public const string ById = "{id:guid}";
    public const string FilteredList = "filtered-list";
    public const string FilteredCount = "filtered-count";
}
