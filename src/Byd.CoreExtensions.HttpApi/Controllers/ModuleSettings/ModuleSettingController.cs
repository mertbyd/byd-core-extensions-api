using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.ModuleSettings;
using Byd.CoreExtensions.Permissions;
using Byd.CoreExtensions.Services.ModuleSettings;
using SystemStandards.Results;

namespace Byd.CoreExtensions.Controllers.ModuleSettings;

/// <summary>
/// Modul ayari CRUD islemlerini HTTP uzerinden sunar.
/// </summary>
// islevi: Route ve request binding metadatasini tasiyip her istegi tek AppService cagrisina yonlendirir.
// sistemdeki gorevi: ModuleSetting tam CRUD dikey kesitinin ince HTTP giris noktasidir.
[ApiController]
[Route(ModuleSettingRoutes.Root)]
public class ModuleSettingController : CoreExtensionsController
{
    /// <summary>ModuleSetting AppService'ini lazy cozer.</summary>
    private IModuleSettingAppService AppService =>
        LazyGetRequiredService<IModuleSettingAppService>();

    /// <summary>
    /// Kimligi verilen modul ayari kaydini getirir.
    /// </summary>
    /// <param name="id">Getirilecek modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id                            (Guid)      → Kayit Id'si
    ///   TenantId                      (Guid?)     → Satirin tenant'i; bos ise host satiridir ve tum tenant'lar icin gecerlidir
    ///   ModuleName                    (string)    → Ayarin ait oldugu modul adi
    ///   IsEnabled                     (bool)      → Modul ayari satirinin etkin olup olmadigi
    ///   TtlSeconds                    (int)       → Ayar satirinin onbellekte tutuldugu sure (saniye); degisiklik en gec bu sure sonunda etkili olur
    ///   RateLimitEnabled              (bool)      → Rate limit acik mi
    ///   RateLimitPermitLimit          (int)       → Pencere basina izin verilen istek sayisi
    ///   RateLimitWindowSeconds        (int)       → Rate limit penceresi (saniye)
    ///   IdempotencyEnabled            (bool)      → Mukerrer istek korumasi acik mi
    ///   IdempotencyTtlSeconds         (int)       → Ayni istegin reddedildigi sure (saniye)
    ///   EventBoxEnabled               (bool)      → Olay kutusu (inbox/outbox) acik mi
    ///   FailedEventRetryUnlimited     (bool)      → Basarisiz olay suresiz mi yeniden denenir
    ///   FailedEventRetryWindowSeconds (int)       → Basarisiz olayin yeniden denendigi pencere (saniye)
    ///   LastModificationTime          (DateTime?) → Son guncelleme zamani
    /// }
    /// Kayit bulunamazsa Manager varlik kontrolu devreye girer ve istek EntityNotFound hata koduyla reddedilir.
    /// </remarks>
    [HttpGet(ModuleSettingRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.View)]
    public virtual async Task<Result<ModuleSettingDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydinin rate limit alanlarini getirir.
    /// </summary>
    /// <param name="id">Rate limit alanlari okunacak modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id                     (Guid)   → Kayit Id'si
    ///   TenantId               (Guid?)  → Satirin tenant'i; bos ise host satiridir ve tum tenant'lar icin gecerlidir
    ///   ModuleName             (string) → Ayarin ait oldugu modul adi
    ///   IsEnabled              (bool)   → Modul ayari satirinin etkin olup olmadigi
    ///   TtlSeconds             (int)    → Ayar satirinin onbellekte tutuldugu sure (saniye); degisiklik en gec bu sure sonunda etkili olur
    ///   RateLimitEnabled       (bool)   → Rate limit acik mi
    ///   RateLimitPermitLimit   (int)    → Pencere basina izin verilen istek sayisi
    ///   RateLimitWindowSeconds (int)    → Rate limit penceresi (saniye)
    /// }
    /// Idempotency ve olay kutusu alanlarini tasimaz.
    /// </remarks>
    [HttpGet(ModuleSettingRoutes.RateLimitById)]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.View)]
    public virtual async Task<Result<ModuleSettingRateLimitDto>> GetRateLimitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetRateLimitAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydinin idempotency alanlarini getirir.
    /// </summary>
    /// <param name="id">Idempotency alanlari okunacak modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id                    (Guid)   → Kayit Id'si
    ///   TenantId              (Guid?)  → Satirin tenant'i; bos ise host satiridir ve tum tenant'lar icin gecerlidir
    ///   ModuleName            (string) → Ayarin ait oldugu modul adi
    ///   IsEnabled             (bool)   → Modul ayari satirinin etkin olup olmadigi
    ///   IdempotencyEnabled    (bool)   → Mukerrer istek korumasi acik mi
    ///   IdempotencyTtlSeconds (int)    → Ayni istegin reddedildigi sure (saniye)
    /// }
    /// Rate limit ve olay kutusu alanlarini tasimaz.
    /// </remarks>
    [HttpGet(ModuleSettingRoutes.IdempotencyById)]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.View)]
    public virtual async Task<Result<ModuleSettingIdempotencyDto>> GetIdempotencyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetIdempotencyAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydinin olay kutusu alanlarini getirir.
    /// </summary>
    /// <param name="id">Olay kutusu alanlari okunacak modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   Id                            (Guid)   → Kayit Id'si
    ///   TenantId                      (Guid?)  → Satirin tenant'i; bos ise host satiridir ve tum tenant'lar icin gecerlidir
    ///   ModuleName                    (string) → Ayarin ait oldugu modul adi
    ///   IsEnabled                     (bool)   → Modul ayari satirinin etkin olup olmadigi
    ///   EventBoxEnabled               (bool)   → Olay kutusu (inbox/outbox) acik mi
    ///   FailedEventRetryUnlimited     (bool)   → Basarisiz olay suresiz mi yeniden denenir
    ///   FailedEventRetryWindowSeconds (int)    → Basarisiz olayin yeniden denendigi pencere (saniye)
    /// }
    /// Rate limit ve idempotency alanlarini tasimaz.
    /// </remarks>
    [HttpGet(ModuleSettingRoutes.EventBoxById)]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.View)]
    public virtual async Task<Result<ModuleSettingEventBoxDto>> GetEventBoxAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetEventBoxAsync(id, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kayitli tum modul ayarlarini tek listede getirir.
    /// </summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response elemanlari tek kayit ucuyla ayni alanlari tasir; kayit yoksa bos liste doner.
    /// Liste sayfalanmaz; tablo her modul icin tek satir tutan kucuk bir yapilandirma kayit defteridir.
    /// </remarks>
    [HttpGet]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.View)]
    public virtual async Task<Result<List<ModuleSettingDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var result = await AppService.GetListAsync(cancellationToken);
        return result;
    }
    /// <summary>
    /// Yeni bir modul icin ayar kaydi olusturur.
    /// </summary>
    /// <param name="input">Modul adi, TTL suresi ve rate limit varsayilanlarini tasiyan create istegi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request {
    ///   TenantId                      (Guid?)  → Satirin tenant'i; bos ise host satiridir ve tum tenant'lar icin gecerlidir
    ///   ModuleName                    (string) → Ayarin ait oldugu modul adi
    ///   IsEnabled                     (bool)   → Modul ayari satirinin etkin olup olmadigi
    ///   TtlSeconds                    (int)    → Ayar satirinin onbellekte tutuldugu sure, 1-86400 saniye
    ///   RateLimitEnabled              (bool)   → Rate limit acik mi
    ///   RateLimitPermitLimit          (int)    → Pencere basina izin verilen istek sayisi, 1-1000000
    ///   RateLimitWindowSeconds        (int)    → Rate limit penceresi, 1-86400 saniye
    ///   IdempotencyEnabled            (bool)   → Mukerrer istek korumasi acik mi
    ///   IdempotencyTtlSeconds         (int)    → Ayni istegin reddedildigi sure, 1-86400 saniye
    ///   EventBoxEnabled               (bool)   → Olay kutusu (inbox/outbox) acik mi
    ///   FailedEventRetryUnlimited     (bool)   → Basarisiz olay suresiz mi yeniden denenir
    ///   FailedEventRetryWindowSeconds (int)    → Basarisiz olayin yeniden denendigi pencere, 1-604800 saniye
    /// }
    /// Response olusan kaydi tek kayit ucuyla ayni alanlarla tasir.
    /// Modul adi Manager tarafinda normalize edilir ve ayni adla ikinci kayit acilmasi engellenir.
    /// </remarks>
    [HttpPost]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.Manage)]
    public virtual async Task<Result<ModuleSettingDto>> CreateAsync(
        [FromBody] CreateModuleSettingDto input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.CreateAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydinin tum alanlarini gunceller.
    /// </summary>
    /// <param name="id">Guncellenecek modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="input">Kaydin yeni modul adi, TTL suresi ve rate limit degerlerini tasiyan update istegi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request olusturma istegiyle ayni alanlari tasir ve kaydin tum alanlarini degistirir; kismi guncelleme yapmaz.
    /// Response guncel kaydi tek kayit ucuyla ayni alanlarla tasir.
    /// Rate limit alanlarindaki degisiklik uygulama yeniden baslatilmadan, en gec kaydin TTL suresi kadar sonra etkili olur.
    /// </remarks>
    [HttpPut(ModuleSettingRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.Manage)]
    public virtual async Task<Result<ModuleSettingDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateModuleSettingDto input,
        CancellationToken cancellationToken = default)
    {
        var result = await AppService.UpdateAsync(id, input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydini kalici olarak siler.
    /// </summary>
    /// <param name="id">Silinecek modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Basarili istekte iceriksiz cevap dondurulur; silinen kayit geri alinmaz.
    /// Silinen modul bir sonraki tazelemede satirsiz kalir ve rate limit'i sabit varsayilanlara duser.
    /// </remarks>
    [HttpDelete(ModuleSettingRoutes.ById)]
    [Authorize(CoreExtensionsPermissions.ModuleSettings.Manage)]
    public virtual async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AppService.DeleteAsync(id, cancellationToken);
        return Result.NoContent();
    }
}
