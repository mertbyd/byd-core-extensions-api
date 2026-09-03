using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Byd.CoreExtensions.Dtos.EventBus;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Services.EventBus;

/// <summary>
/// Basarisiz olay kutusu okuma kullanim senaryolarini tanimlar.
/// </summary>
// islevi: Tekil okuma, filtreli liste ve filtreli sayim kontratini acar.
// sistemdeki gorevi: Controller ile Application implementasyonu arasindaki public servis siniridir.
public interface IFailedEventInboxAppService : IApplicationService
{
    // Kimligi verilen basarisiz olay kaydini getirir.
    Task<FailedEventInboxDto> GetAsync(Guid id);

    // Filtre, siralama ve sayfalama uygulanmis basarisiz olay listesini getirir.
    Task<List<FailedEventInboxDto>> GetFilteredListAsync(FailedEventInboxListInput input);

    // Ayni filtreye uyan toplam kayit sayisini getirir.
    Task<int> GetFilteredCountAsync(FailedEventInboxListInput input);
}
