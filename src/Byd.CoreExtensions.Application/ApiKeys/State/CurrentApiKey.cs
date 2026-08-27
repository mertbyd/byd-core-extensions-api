using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Icinde bulunulan istegi dogrulayan API anahtarinin request kapsamli bellek durumu.
/// </summary>
// islevi: Middleware'in yazdigi anahtar goruntusunu istek suresince tutar; dogrulama yapilmadiysa null kalir.
// sistemdeki gorevi: ICurrentApiKey'in scoped uygulamasi; middleware Set ile yazar, downstream kod arayuz uzerinden okur.
public class CurrentApiKey : ICurrentApiKey, IScopedDependency
{
    // Downstream kodun "bu istegi hangi anahtar yapti" sorusunu okudugu guncel goruntu.
    public ApiKeyDescriptor? Current { get; private set; }

    // Basarili dogrulamada anahtar goruntusunu istek baglamina yazar; yalnizca dogrulama middleware'i cagirir.
    public void Set(ApiKeyDescriptor apiKey)
    {
        Current = apiKey;
    }
}
