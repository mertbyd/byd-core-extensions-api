using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Querying;

namespace Byd.CoreExtensions.Dtos.EventBus;

/// <summary>
/// Basarisiz olay kutusunun operasyonel filtre alanlarini tasir.
/// </summary>
// islevi: Her filtre propertysini kararli tel adi ve entity property yoluyla DataTable motoruna tanitir.
// sistemdeki gorevi: Basarisiz olay liste ve sayim uclarinin tipli filtre izin listesidir.
public class FailedEventInboxFilter
{
    [DataTable(FailedEventInboxFilterFields.MessageId, FailedEventInboxFilterFields.PropertyPaths.MessageId)]
    public DataTableFilterItem? MessageId { get; set; }

    [DataTable(FailedEventInboxFilterFields.EventName, FailedEventInboxFilterFields.PropertyPaths.EventName)]
    public DataTableFilterItem? EventName { get; set; }

    [DataTable(FailedEventInboxFilterFields.OwnerModuleName, FailedEventInboxFilterFields.PropertyPaths.OwnerModuleName)]
    public DataTableFilterItem? OwnerModuleName { get; set; }

    [DataTable(FailedEventInboxFilterFields.OwnerEnvironmentName, FailedEventInboxFilterFields.PropertyPaths.OwnerEnvironmentName)]
    public DataTableFilterItem? OwnerEnvironmentName { get; set; }

    [DataTable(FailedEventInboxFilterFields.IsProcessed, FailedEventInboxFilterFields.PropertyPaths.IsProcessed)]
    public DataTableFilterItem? IsProcessed { get; set; }

    [DataTable(FailedEventInboxFilterFields.RetryCount, FailedEventInboxFilterFields.PropertyPaths.RetryCount)]
    public DataTableFilterItem? RetryCount { get; set; }

    [DataTable(FailedEventInboxFilterFields.NotBefore, FailedEventInboxFilterFields.PropertyPaths.NotBefore)]
    public DataTableFilterItem? NotBefore { get; set; }

    [DataTable(FailedEventInboxFilterFields.CreationTime, FailedEventInboxFilterFields.PropertyPaths.CreationTime)]
    public DataTableFilterItem? CreationTime { get; set; }
}
