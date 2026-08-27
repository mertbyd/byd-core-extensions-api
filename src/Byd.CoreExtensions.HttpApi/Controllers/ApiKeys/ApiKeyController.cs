using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.ApiKeys;
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
public class ApiKeyController : CoreExtensionsController//todo : auth module gelince buraya permission atilacak, su an test edebilmek icin atilmadi
{
    /// <summary>ApiKey AppService'ini lazy cozer.</summary>
    private IApiKeyAppService AppService =>
        LazyGetRequiredService<IApiKeyAppService>();

    /// <summary>
    /// Kimligi verilen API anahtari kaydini getirir.
    /// Cevap anahtar degerini tasimaz; ham anahtar yalnizca uretme cevabinda bir kez doner.
    /// Kayit bulunamazsa Manager varlik kontrolu devreye girer ve istek EntityNotFound hata koduyla reddedilir.
    /// </summary>
    /// <param name="id">Getirilecek API anahtari kaydinin benzersiz kimligi.</param>
    /// <returns>Ad, aktiflik, son kullanma ve kapsam alanlariyla tek API anahtari kaydi.</returns>
    [HttpGet(ApiKeyRoutes.ById)]
    public virtual async Task<Result<ApiKeyDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }
    /// <summary>
    /// Yeni bir API anahtari uretir.
    /// Anahtar degerini sunucu uretir ve yalnizca bu cevapta doner; saklanmasi cagiranin sorumlulugundadir.
    /// Kaydin tenant ve organizasyon alanlari istegin guncel kapsamindan damgalanir.
    /// </summary>
    /// <param name="input">Anahtarin adini, aktifligini ve istege bagli son kullanma zamanini tasiyan uretme istegi.</param>
    /// <returns>Uretilen kimlik, kapsam alanlari ve tek seferlik ham anahtar degeri.</returns>
    [HttpPost]
    public virtual async Task<Result<CreatedApiKeyDto>> Create(
        [FromBody] CreateApiKeyDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }
}
