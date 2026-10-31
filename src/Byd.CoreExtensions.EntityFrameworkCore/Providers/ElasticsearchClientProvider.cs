using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Byd.CoreExtensions.Providers;

/// <summary>
/// Elasticsearch istemcisini ayarlardan kurup host boyunca paylastirir.
/// </summary>
// islevi: Baglanti ayarlarini her istekte okur; ayarlar degismediyse ayni istemciyi, degistiyse yeni kurulan istemciyi verir.
// Elasticsearch kapaliysa istemci yerine kapali oldugunu soyleyen hata doner.
// sistemdeki gorevi: Arama repository'sinin ve saglik yoklamasinin istemci kaynagi; ayar ekrandan degistirildiginde host
// yeniden baslatilmadan yeni kumeye gecilir. Belgeler ev seklindeki ic setter'lariyla da okunabilsin diye seri hale getirici
// ic uyeleri de doldurur.
public class ElasticsearchClientProvider(ISettingProvider settingProvider) : ISingletonDependency
{
    // Belgeleri ve onbellekteki arama sonucunu ayni kurallarla yazip okuyan JSON secenekleri.
    public static readonly JsonSerializerOptions DocumentSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { IncludeNonPublicMembers } }
    };

    private readonly Lock _lock = new();
    private string? _fingerprint;
    private ElasticsearchClient? _client;

    // Ayarlar degismediyse mevcut istemciyi, degistiyse yenisini dondurur.
    public virtual async Task<ElasticsearchClient> GetClientAsync(CancellationToken cancellationToken = default)
    {
        if (!await settingProvider.IsTrueAsync(CoreExtensionsSettings.Elasticsearch.IsEnabled))
            throw new BusinessException(ElasticsearchExceptionCodes.NotEnabled);

        var url = await settingProvider.GetOrNullAsync(CoreExtensionsSettings.Elasticsearch.Url);
        var userName = await settingProvider.GetOrNullAsync(CoreExtensionsSettings.Elasticsearch.UserName);
        var password = await settingProvider.GetOrNullAsync(CoreExtensionsSettings.Elasticsearch.Password);
        var isTraceMode = await settingProvider.IsTrueAsync(CoreExtensionsSettings.Elasticsearch.IsTraceMode);
        var fingerprint = string.Join('|', url, userName, password, isTraceMode);
        lock (_lock)
        {
            if (_client is null || _fingerprint != fingerprint)
            {
                _client = CreateClient(url, userName, password, isTraceMode);
                _fingerprint = fingerprint;
            }
            return _client;
        }
    }

    // Istemciyi kurar; basarisiz yanitlar sessizce donmek yerine istisna olarak cagirana tasinir.
    private static ElasticsearchClient CreateClient(string? url, string? userName, string? password, bool isTraceMode)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new BusinessException(ElasticsearchExceptionCodes.NotEnabled);

        var settings = new ElasticsearchClientSettings(
                new SingleNodePool(new Uri(url)),
                (_, clientSettings) => new DefaultSourceSerializer(
                    clientSettings,
                    DocumentSerializerOptions.TypeInfoResolver!,
                    options => options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase))
            .ThrowExceptions();
        if (!string.IsNullOrWhiteSpace(userName))
            settings.Authentication(new BasicAuthentication(userName, password ?? string.Empty));
        if (isTraceMode)
            settings.EnableDebugMode();
        return new ElasticsearchClient(settings);
    }

    // Korumali parametresiz ctor'u ve public olmayan setter'lari seri hale getiriciye acar; boylece internal set ve korumali
    // ctor tasiyan ev seklindeki tipler de geri okunur.
    private static void IncludeNonPublicMembers(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;
        if (typeInfo.CreateObject is null
            && typeInfo.Type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes) is { } constructor)
            typeInfo.CreateObject = () => constructor.Invoke(null);
        foreach (var property in typeInfo.Properties)
        {
            if (property.Set is null && property.AttributeProvider is PropertyInfo { SetMethod: { } setter })
                property.Set = (target, value) => setter.Invoke(target, [value]);
        }
    }
}
