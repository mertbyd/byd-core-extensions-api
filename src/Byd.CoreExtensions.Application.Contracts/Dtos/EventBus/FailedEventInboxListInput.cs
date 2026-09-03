using Byd.CoreExtensions.Querying;

namespace Byd.CoreExtensions.Dtos.EventBus;

/// <summary>
/// Basarisiz olay kutusu filtre, siralama ve sayfalama istegini tasir.
/// </summary>
// islevi: Tipli filtre modelini DataTablePaging ile tek public inputta birlestirir.
// sistemdeki gorevi: Filtreli liste ve sayim endpointlerinin dogrulanan request sozlesmesidir.
public class FailedEventInboxListInput
    : DataTablePaging, IDataTablePagingProperty<FailedEventInboxFilter>
{
    public FailedEventInboxFilter? Filters { get; set; }
}
