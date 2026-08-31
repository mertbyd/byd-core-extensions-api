using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.ModuleSettings;
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
    /// Kayit bulunamazsa Manager varlik kontrolu devreye girer ve istek EntityNotFound hata koduyla reddedilir.
    /// </summary>
    /// <param name="id">Getirilecek modul ayari kaydinin benzersiz kimligi.</param>
    /// <returns>Modul adi, TTL suresi ve rate limit alanlariyla birlikte tek modul ayari kaydi.</returns>
    [HttpGet(ModuleSettingRoutes.ById)]
    public virtual async Task<Result<ModuleSettingDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }
    /// <summary>
    /// Kayitli tum modul ayarlarini tek listede getirir.
    /// Liste sayfalanmaz; tablo her modul icin tek satir tutan kucuk bir yapilandirma kayit defteridir.
    /// </summary>
    /// <returns>Her modulun ayar kaydini iceren tam liste; kayit yoksa bos liste.</returns>
    [HttpGet]
    public virtual async Task<Result<List<ModuleSettingDto>>> GetList()
    {
        var result = await AppService.GetListAsync();
        return result;
    }
    /// <summary>
    /// Yeni bir modul icin ayar kaydi olusturur.
    /// Modul adi Manager tarafinda normalize edilir ve ayni adla ikinci kayit acilmasi engellenir.
    /// </summary>
    /// <param name="input">Modul adi, TTL suresi ve rate limit varsayilanlarini tasiyan create istegi.</param>
    /// <returns>Uretilen kimlik ve kalici hale gelmis alanlarla birlikte yeni modul ayari kaydi.</returns>
    [HttpPost]
    public virtual async Task<Result<ModuleSettingDto>> Create(
        [FromBody] CreateModuleSettingDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydinin tum alanlarini gunceller.
    /// Rate limit alanlarindaki degisiklik uygulama yeniden baslatilmadan, en gec kaydin TTL suresi kadar sonra etkili olur.
    /// </summary>
    /// <param name="id">Guncellenecek modul ayari kaydinin benzersiz kimligi.</param>
    /// <param name="input">Kaydin yeni modul adi, TTL suresi ve rate limit degerlerini tasiyan update istegi.</param>
    /// <returns>Guncel alanlari ve yeni son degistirilme zamanini tasiyan modul ayari kaydi.</returns>
    [HttpPut(ModuleSettingRoutes.ById)]
    public virtual async Task<Result<ModuleSettingDto>> Update(
        Guid id,
        [FromBody] UpdateModuleSettingDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }
    /// <summary>
    /// Kimligi verilen modul ayari kaydini kalici olarak siler.
    /// Silinen modul bir sonraki tazelemede satirsiz kalir ve rate limit'i sabit varsayilanlara duser.
    /// </summary>
    /// <param name="id">Silinecek modul ayari kaydinin benzersiz kimligi.</param>
    /// <returns>Govdesiz basari sonucu.</returns>
    [HttpDelete(ModuleSettingRoutes.ById)]
    public virtual async Task<Result> Delete(Guid id)
    {
        await AppService.DeleteAsync(id);
        return Result.NoContent();
    }
}
