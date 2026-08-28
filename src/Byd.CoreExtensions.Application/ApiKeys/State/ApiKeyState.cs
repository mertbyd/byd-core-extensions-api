using System;
using System.Collections.Generic;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Tum API anahtarlarinin senkron okunabilen, versiyonlu bellek durumu.
/// </summary>
// islevi: Guncel snapshot'i tutar; tazeleme yapildiginda yeni versiyonlu snapshot ile atomik olarak degistirir.
// sistemdeki gorevi: Anahtar dogrulamasinin kalbi; dogrulama bu durumu kilitlemeden okur, tazeleyici ise Update ile yeniler.
// mirastan gelen: yok - durum tasiyicisi oldugu icin taban sinif kullanilmaz; yasam dongusu ISingletonDependency ile ABP'ye birakilir.
// burada yazilan: Current, Update
public class ApiKeyState : IApiKeyState, ISingletonDependency
{
    // Baslangic goruntusu bostur ve yuklenme zamani DateTime.MinValue'dur; bu sayede ilk dogrulama
    // isteginde goruntu bayat sayilir ve tablo okunur. Boylece baslangic yuklemesi atlansa bile
    // sistem kendini toparlar.
    private volatile ApiKeySnapshot _current = new(
        new Dictionary<string, ApiKeyDescriptor>(StringComparer.Ordinal),
        loadedAt: DateTime.MinValue,
        version: 0);

    // Dogrulamanin her istekte senkron okudugu guncel snapshot.
    public ApiKeySnapshot Current => _current;

    // Tablodan okunan anahtar kumesini bir sonraki versiyonla snapshot'a yazar; okuma tarafi kilitlenmez.
    public void Update(IReadOnlyDictionary<string, ApiKeyDescriptor> apiKeys, DateTime loadedAt)
    {
        _current = new ApiKeySnapshot(apiKeys, loadedAt, _current.Version + 1);
    }
}
