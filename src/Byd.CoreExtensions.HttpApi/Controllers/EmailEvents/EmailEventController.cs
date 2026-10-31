using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.EmailEvents;
using Byd.CoreExtensions.Permissions;
using Byd.CoreExtensions.Services.EmailEvents;
using SystemStandards.Results;

namespace Byd.CoreExtensions.Controllers.EmailEvents;

/// <summary>
/// E-posta olayi okuma islemlerini HTTP uzerinden sunar.
/// </summary>
// islevi: Route ve request binding metadatasini tasiyip her istegi tek AppService cagrisina yonlendirir.
// sistemdeki gorevi: Sablon olustururken secilecek olaylari listeleyen ince HTTP giris noktasidir.
[ApiController]
[Route(EmailEventRoutes.Root)]
public class EmailEventController : CoreExtensionsController
{
    /// <summary>EmailEvent AppService'ini lazy cozer.</summary>
    private IEmailEventAppService AppService =>
        LazyGetRequiredService<IEmailEventAppService>();

    /// <summary>
    /// Kimligi verilen e-posta olayini getirir.
    /// </summary>
    /// <param name="id">Getirilecek olayin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id           (Guid)     → Kayit Id'si
    ///   Name         (string)   → Olayin tekil adi; olayi gonderen modulun sabitidir
    ///   Code         (int?)     → Istege bagli olay kodu
    ///   CreationTime (DateTime) → Olusturulma zamani
    /// }
    /// Kayit bulunamazsa istek EntityNotFound hata koduyla reddedilir. Olaylari onlari gonderen moduller seed eder;
    /// bu uc olay olusturmaz ve degistirmez.
    /// </remarks>
    [HttpGet(EmailEventRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.View)]
    public virtual async Task<Result<EmailEventDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Seed edilmis tum e-posta olaylarini getirir.
    /// </summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response elemanlari tek kayit ucuyla ayni alanlari tasir; olay yoksa bos liste doner.
    /// Liste sablon olustururken EmailEventId secmek icindir; sayfalanmaz, olay olusturmaz ve degistirmez.
    /// </remarks>
    [HttpGet]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.View)]
    public virtual async Task<Result<List<EmailEventDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetListAsync(cancellationToken);
        return result;
    }
}
