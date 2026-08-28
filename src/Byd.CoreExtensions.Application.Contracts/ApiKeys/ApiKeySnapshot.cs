using System;
using System.Collections.Generic;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Tum API anahtarlarinin belirli bir versiyondaki degismez anlik goruntusu.
/// </summary>
// islevi: Anahtar degerinden cozulmus goruntuye eslemeyi, yuklenme zamanini ve versiyonu degismez tek nesnede tasir.
// sistemdeki gorevi: Dogrulamanin senkron okudugu durum sozlesmesi. Anahtarlarin ham degerleri yalnizca sozluk anahtari olarak yasar ve disari acilmaz; erisim tek bir arama metoduyla verilir.
public sealed class ApiKeySnapshot
{
    private readonly IReadOnlyDictionary<string, ApiKeyDescriptor> _apiKeys;

    public ApiKeySnapshot(
        IReadOnlyDictionary<string, ApiKeyDescriptor> apiKeys,
        DateTime loadedAt,
        long version)
    {
        _apiKeys = apiKeys;
        LoadedAt = loadedAt;
        Version = version;
    }

    // Bu goruntunun tablodan yuklendigi zaman (UTC); tazelik karari buna gore verilir.
    public DateTime LoadedAt { get; }

    // Bu anlik goruntunun versiyonu; her tazelemede artar.
    public long Version { get; }

    // Goruntudeki anahtar sayisi.
    public int Count => _apiKeys.Count;

    // Verilen anahtar degerinin cozulmus goruntusunu doner; goruntude yoksa null doner.
    public ApiKeyDescriptor? Find(string key)
    {
        return _apiKeys.TryGetValue(key, out var descriptor) ? descriptor : null;
    }
}
