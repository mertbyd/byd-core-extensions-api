using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.EventBus;
using Byd.CoreExtensions.Services.EventBus;
using SystemStandards.Results;

namespace Byd.CoreExtensions.Controllers.EventBus;

/// <summary>
/// Basarisiz olay kutusu okuma islemlerini HTTP uzerinden sunar.
/// </summary>
// islevi: Route ve request binding metadatasini tasiyip her istegi tek AppService cagrisina yonlendirir.
// sistemdeki gorevi: Basarisiz olay kutusu dikey kesitinin salt okunur ince HTTP giris noktasidir.
[ApiController]
[Route(FailedEventInboxRoutes.Root)]
public class FailedEventInboxController : CoreExtensionsController//todo : auth module gelince buraya permission atilacak, su an test edebilmek icin atilmadi
{
    /// <summary>FailedEventInbox AppService'ini lazy cozer.</summary>
    private IFailedEventInboxAppService AppService =>
        LazyGetRequiredService<IFailedEventInboxAppService>();
    /// <summary>
    /// Kimligi verilen basarisiz olay kutusu kaydini getirir.
    /// Olay govdesi cevap sozlesmesine dahil edilmez.
    /// </summary>
    /// <param name="id">Getirilecek basarisiz olay kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <returns>Olay kapsami, isleme durumu ve hata bilgisiyle tek basarisiz olay kaydi.</returns>
    // Kimligi verilen basarisiz olay kaydini getirir.
    [HttpGet(FailedEventInboxRoutes.ById)]
    public virtual async Task<Result<FailedEventInboxDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Filtre, siralama ve sayfalama uygulanmis basarisiz olay listesini getirir.
    /// </summary>
    /// <param name="input">Filtre gruplari, siralama ve sayfalama girdisi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <returns>Filtre kosullarini saglayan basarisiz olay kayitlari.</returns>
    // Filtreli ve sayfalanmis basarisiz olay listesini getirir.
    [HttpPost(FailedEventInboxRoutes.FilteredList)]
    public virtual async Task<Result<List<FailedEventInboxDto>>> GetFilteredListAsync(
        [FromBody] FailedEventInboxListInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetFilteredListAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Filtre kosullarini saglayan basarisiz olay kaydi sayisini getirir.
    /// </summary>
    /// <param name="input">Sayima uygulanacak filtre gruplari.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <returns>Filtrelenmis basarisiz olay kaydi sayisi.</returns>
    // Ayni filtreye uyan toplam basarisiz olay kaydi sayisini getirir.
    [HttpPost(FailedEventInboxRoutes.FilteredCount)]
    public virtual async Task<Result<int>> GetFilteredCountAsync(
        [FromBody] FailedEventInboxListInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetFilteredCountAsync(input, cancellationToken);
        return result;
    }
}
