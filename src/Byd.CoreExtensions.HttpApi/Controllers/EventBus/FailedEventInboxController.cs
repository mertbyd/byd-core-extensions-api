using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.EventBus;
using Byd.CoreExtensions.Permissions;
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
public class FailedEventInboxController : CoreExtensionsController
{
    /// <summary>FailedEventInbox AppService'ini lazy cozer.</summary>
    private IFailedEventInboxAppService AppService =>
        LazyGetRequiredService<IFailedEventInboxAppService>();
    /// <summary>
    /// Kimligi verilen basarisiz olay kutusu kaydini getirir.
    /// </summary>
    /// <param name="id">Getirilecek basarisiz olay kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id                   (Guid)      → Kayit Id'si
    ///   MessageId            (string)    → Olayin mesaj kimligi
    ///   EventName            (string)    → Olay adi
    ///   OwnerModuleName      (string)    → Olayi isleyen modul
    ///   OwnerEnvironmentName (string)    → Olayi isleyen ortam
    ///   IsProcessed          (bool)      → Yeniden denemede basariyla islendi mi
    ///   ProcessedTime        (DateTime?) → Islendigi zaman
    ///   NotBefore            (DateTime)  → Bir sonraki denemenin en erken zamani
    ///   RetryCount           (int)       → Yapilan deneme sayisi
    ///   ExceptionMessage     (string?)   → Son hatanin mesaji
    ///   ExceptionStackTrace  (string?)   → Son hatanin yigin izi
    ///   CreationTime         (DateTime)  → Kaydin olusma zamani
    ///   LastModificationTime (DateTime?) → Son guncelleme zamani
    /// }
    /// Olay bu uctan yeniden denenmez; uc yalniz okur.
    /// Olay govdesi cevap sozlesmesine dahil edilmez.
    /// </remarks>
    [HttpGet(FailedEventInboxRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.FailedEventInboxes.View)]
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
    /// <remarks>
    /// Request {
    ///   Filters                (FailedEventInboxFilter?) → Alan bazli filtreler; her alan { Operator, Value, SecondValue, Values } tasir
    ///     MessageId            (DataTableFilterItem?)    → Mesaj kimligi filtresi
    ///     EventName            (DataTableFilterItem?)    → Olay adi filtresi
    ///     OwnerModuleName      (DataTableFilterItem?)    → Modul filtresi
    ///     OwnerEnvironmentName (DataTableFilterItem?)    → Ortam filtresi
    ///     IsProcessed          (DataTableFilterItem?)    → Islenme durumu filtresi
    ///     RetryCount           (DataTableFilterItem?)    → Deneme sayisi filtresi
    ///     NotBefore            (DataTableFilterItem?)    → Sonraki deneme zamani filtresi
    ///     CreationTime         (DataTableFilterItem?)    → Olusma zamani filtresi
    /// }
    /// Response elemanlari tek kayit ucuyla ayni alanlari tasir.
    /// </remarks>
    [HttpPost(FailedEventInboxRoutes.FilteredList)]
    [Authorize(CoreExtensionsPermissions.FailedEventInboxes.View)]
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
    /// <remarks>
    /// Request filtreli liste ucuyla aynidir. Response filtreye uyan kayit sayisidir (int); satirlari dondurmez.
    /// </remarks>
    [HttpPost(FailedEventInboxRoutes.FilteredCount)]
    [Authorize(CoreExtensionsPermissions.FailedEventInboxes.View)]
    public virtual async Task<Result<int>> GetFilteredCountAsync(
        [FromBody] FailedEventInboxListInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetFilteredCountAsync(input, cancellationToken);
        return result;
    }
}
