using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.EmailTemplates;
using Byd.CoreExtensions.Permissions;
using Byd.CoreExtensions.Services.EmailTemplates;
using SystemStandards.Results;

namespace Byd.CoreExtensions.Controllers.EmailTemplates;

/// <summary>
/// E-posta sablonu CRUD ve aktiflestirme islemlerini HTTP uzerinden sunar.
/// </summary>
// islevi: Route ve request binding metadatasini tasiyip her istegi tek AppService cagrisina yonlendirir.
// sistemdeki gorevi: EmailTemplate dikey kesitinin ince HTTP giris noktasidir.
[ApiController]
[Route(EmailTemplateRoutes.Root)]
public class EmailTemplateController : CoreExtensionsController
{
    /// <summary>EmailTemplate AppService'ini lazy cozer.</summary>
    private IEmailTemplateAppService AppService =>
        LazyGetRequiredService<IEmailTemplateAppService>();

    /// <summary>
    /// Kimligi verilen e-posta sablonunu getirir.
    /// </summary>
    /// <param name="id">Getirilecek sablonun benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id                   (Guid)      → Kayit Id'si
    ///   EmailEventId         (Guid)      → Sablonun bagli oldugu e-posta olayinin Id'si
    ///   EmailEventName       (string)    → Olayin tekil adi
    ///   Name                 (string)    → Surumleri ayirt etmek icin verilen sablon adi
    ///   CultureName          (string)    → Sablonun dili (ornek tr, en)
    ///   Subject              (string)    → E-posta konusu sablonu
    ///   IsActive             (bool)      → Bu olay ve dil icin kapsamin gecerli surumu mu
    ///   CreationTime         (DateTime)  → Olusturulma zamani
    ///   LastModificationTime (DateTime?) → Son guncelleme zamani
    ///   Body                 (string)    → E-posta icerigi sablonu (HTML)
    /// }
    /// Kayit istegi yapanin kapsaminda aranir; ust kapsamin veya baska kapsamin sablonu EntityNotFound hata koduyla reddedilir.
    /// </remarks>
    [HttpGet(EmailTemplateRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.View)]
    public virtual async Task<Result<EmailTemplateDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Istegi yapanin kapsamindaki tum e-posta sablonu surumlerini getirir.
    /// </summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response elemanlari tek kayit ucuyla ayni alanlari tasir; kapsamda sablon yoksa bos liste doner.
    /// Ust organizasyonun, tenant'in veya host'un sablonlari listelenmez; gonderimde bu kapsamlara dusulse de yonetimleri kendi kapsamlarindadir.
    /// </remarks>
    [HttpGet]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.View)]
    public virtual async Task<Result<List<EmailTemplateDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetListAsync(cancellationToken);
        return result;
    }
    /// <summary>
    /// Istegi yapanin kapsaminda yeni bir e-posta sablonu olusturur.
    /// </summary>
    /// <param name="input">Olay, dil, ad, konu ve icerigi tasiyan create istegi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request {
    ///   EmailEventId (Guid)   → Sablonun bagli oldugu e-posta olayinin Id'si; olaylar email-events ucundan listelenir
    ///   CultureName  (string) → Sablonun dili, en fazla 10 karakter (ornek tr, en)
    ///   Name         (string) → Sablon adi, en fazla 128 karakter
    ///   Subject      (string) → E-posta konusu sablonu, en fazla 256 karakter
    ///   Body         (string) → E-posta icerigi sablonu (HTML); degiskenler {{ ad }} biciminde yazilir
    /// }
    /// Response olusan kaydi tek kayit ucuyla ayni alanlarla tasir.
    /// Sablon istegi yapanin tenant ve organizasyonuna damgalanir ve aktif olarak acilir; ayni kapsamda bu olay ve dilin onceki aktif surumu pasife alinir.
    /// Olay bulunamazsa EntityNotFound, konu veya icerikteki sablon soz dizimi hataliysa soz dizimi hatasiyla kayit acilmaz.
    /// </remarks>
    [HttpPost]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.Manage)]
    public virtual async Task<Result<EmailTemplateDto>> CreateAsync(
        [FromBody] CreateEmailTemplateDto input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.CreateAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen e-posta sablonunun ad, konu ve icerigini gunceller.
    /// </summary>
    /// <param name="id">Guncellenecek sablonun benzersiz kimligi.</param>
    /// <param name="input">Sablonun yeni ad, konu ve icerigini tasiyan update istegi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request {
    ///   Name    (string) → Sablon adi, en fazla 128 karakter
    ///   Subject (string) → E-posta konusu sablonu, en fazla 256 karakter
    ///   Body    (string) → E-posta icerigi sablonu (HTML)
    /// }
    /// Response guncel kaydi tek kayit ucuyla ayni alanlarla tasir.
    /// Olay, dil, kapsam ve aktiflik bu uctan degismez; aktif surum degistirmek icin aktiflestirme ucu kullanilir.
    /// </remarks>
    [HttpPut(EmailTemplateRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.Manage)]
    public virtual async Task<Result<EmailTemplateDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateEmailTemplateDto input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.UpdateAsync(id, input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen pasif e-posta sablonunu kalici olarak siler.
    /// </summary>
    /// <param name="id">Silinecek sablonun benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Basarili istekte iceriksiz cevap dondurulur; silinen kayit geri alinmaz.
    /// Aktif sablon silinmez; once ayni olay ve dilde baska bir surum aktif yapilmalidir.
    /// </remarks>
    [HttpDelete(EmailTemplateRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.Manage)]
    public virtual async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AppService.DeleteAsync(id, cancellationToken);
        return Result.NoContent();
    }
    /// <summary>
    /// Kimligi verilen pasif e-posta sablonunu kendi olay ve dilinde aktif surum yapar.
    /// </summary>
    /// <param name="id">Aktif yapilacak sablonun benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response aktif yapilan kaydi tek kayit ucuyla ayni alanlarla tasir.
    /// Ayni kapsamda bu olay ve dilin onceki aktif surumu pasife alinir; zaten aktif olan sablon icin istek reddedilir.
    /// </remarks>
    [HttpPost(EmailTemplateRoutes.ActivateById)]
    [Authorize(CoreExtensionsPermissions.EmailTemplates.Manage)]
    public virtual async Task<Result<EmailTemplateDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.ActivateAsync(id, cancellationToken);
        return result;
    }
}
