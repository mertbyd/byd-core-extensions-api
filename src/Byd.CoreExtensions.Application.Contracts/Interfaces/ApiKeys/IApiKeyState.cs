using Byd.CoreExtensions.ApiKeys;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Bellekteki API anahtari goruntusunun guncel anlik halini sunar.
/// </summary>
// islevi: Dogrulama tarafinin Application state uygulamasina baglanmadan guncel snapshot'i okumasini saglar.
// sistemdeki gorevi: Anahtar durumunun yonetimi ile okunmasi arasindaki tek yonlu contract siniridir; yazma yetkisi yalnizca Application tarafindaki uygulamadadir.
public interface IApiKeyState
{
    ApiKeySnapshot Current { get; }
}
