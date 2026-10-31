using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.ApiKeys;
using Byd.CoreExtensions.Permissions;
using Byd.CoreExtensions.Services.ApiKeys;
using SystemStandards.Results;

namespace Byd.CoreExtensions.Controllers.ApiKeys;

/// <summary>
/// API anahtari okuma ve uretme islemlerini HTTP uzerinden sunar.
/// </summary>
// islevi: Route ve request binding metadatasini tasiyip her istegi tek AppService cagrisina yonlendirir.
// sistemdeki gorevi: ApiKey dikey kesitinin ince HTTP giris noktasidir; anahtarlar guncellenmez ve silinmez.
[ApiController]
[Route(ApiKeyRoutes.Root)]
public class ApiKeyController : CoreExtensionsController
{
    /// <summary>ApiKey AppService'ini lazy cozer.</summary>
    private IApiKeyAppService AppService =>
        LazyGetRequiredService<IApiKeyAppService>();

    /// <summary>
    /// Kimligi verilen API anahtari kaydini getirir.
    /// </summary>
    /// <param name="id">Getirilecek API anahtari kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id             (Guid)      → Kayit Id'si
    ///   TenantId       (Guid?)     → Anahtarin adina calistigi tenant; bos ise host
    ///   OrganizationId (Guid?)     → Anahtarin adina calistigi organizasyon
    ///   Name           (string)    → Anahtarin gorunen adi
    ///   ExpiresAt      (DateTime?) → Son kullanma zamani; bos ise suresiz
    ///   CreationTime   (DateTime)  → Olusturulma zamani
    /// }
    /// Ham anahtar degeri bu ucta donmez.
    /// Cevap anahtar degerini tasimaz; ham anahtar yalnizca uretme cevabinda bir kez doner.
    /// Kayit bulunamazsa Manager varlik kontrolu devreye girer ve istek EntityNotFound hata koduyla reddedilir.
    /// </remarks>
    [HttpGet(ApiKeyRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.ApiKeys.View)]
    public virtual async Task<Result<ApiKeyDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Yeni bir API anahtari uretir.
    /// </summary>
    /// <param name="input">Anahtarin kapsamini, adini ve istege bagli son kullanma zamanini tasiyan uretme istegi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request {
    ///   TenantId       (Guid?)     → Anahtarin adina calisacagi tenant; bos ise host
    ///   OrganizationId (Guid?)     → Anahtarin adina calisacagi organizasyon
    ///   Name           (string)    → Anahtarin gorunen adi
    ///   ExpiresAt      (DateTime?) → Son kullanma zamani; bos ise suresiz
    /// }
    /// Response {
    ///   Id             (Guid)      → Kayit Id'si
    ///   TenantId       (Guid?)     → Anahtarin adina calistigi tenant; bos ise host
    ///   OrganizationId (Guid?)     → Anahtarin adina calistigi organizasyon
    ///   Name           (string)    → Anahtarin gorunen adi
    ///   ExpiresAt      (DateTime?) → Son kullanma zamani; bos ise suresiz
    ///   CreationTime   (DateTime)  → Olusturulma zamani
    ///   Key            (string)    → Ham anahtar degeri
    /// }
    /// Ham anahtar yalniz bu cevapta bir kez gosterilir, sonra okunamaz; saklanmasi cagiranin sorumlulugundadir.
    /// Anahtar degerini sunucu uretir ve yalnizca bu cevapta doner; saklanmasi cagiranin sorumlulugundadir.
    /// Tenant ve organizasyon alanlari istekten gelir; anahtarin hangi kapsam adina calisacagini cagiran belirler.
    /// </remarks>
    [HttpPost]
    [Authorize(CoreExtensionsPermissions.ApiKeys.Manage)]
    public virtual async Task<Result<CreatedApiKeyDto>> CreateAsync(
        [FromBody] CreateApiKeyDto input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.CreateAsync(input, cancellationToken);
        return result;
    }
}
