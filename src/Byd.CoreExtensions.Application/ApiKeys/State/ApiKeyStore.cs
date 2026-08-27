using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Mappers.ApiKeys;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Gelen API anahtarini dogrular ve cozulmus anahtarlari surec omru boyunca bellekte tutar.
/// </summary>
// islevi: Anahtari once bellekteki sozlukte arar; bulamazsa repository'den tek kayit okur, bellege alir ve son kullanma kontrolunu yapar.
// sistemdeki gorevi: Anahtar dogrulamasinin tek okuma noktasi. Tablonun tamami hic yuklenmez; bellek yalnizca gercekten kullanilan anahtarlar kadar buyur.
public class ApiKeyStore : IApiKeyStore, ITransientDependency
{
    // Cozulmus anahtar goruntuleri. Statiktir: durum servis ornegine degil surece aittir, boylece
    // istekler arasinda yasar. Yalnizca veritabaninda bulunan anahtarlar yazilir; bilinmeyen anahtar
    // yazilmaz, aksi halde rastgele anahtar denemeleri sozlugu sinirsiz sisirebilirdi.
    // Bellek surece ozeldir ve surec omru boyunca yasar; anahtar guncelleme/silme endpointi
    // bulunmadigi icin cozulmus goruntuyu dusuren bir yol da yoktur.
    private static readonly ConcurrentDictionary<string, ApiKeyDescriptor> ResolvedApiKeys =
        new(StringComparer.Ordinal);

    private static readonly ApiKeyMapper Mapper = new();

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IClock _clock;

    public ApiKeyStore(IApiKeyRepository apiKeyRepository, IClock clock)
    {
        _apiKeyRepository = apiKeyRepository;
        _clock = clock;
    }

    // Gelen anahtari cozer ve dogrular; ilk saglanmayan kosulun hata kodunu doner.
    public async Task<ApiKeyValidationResult> ValidateAsync(
        string? key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return ApiKeyValidationResult.Fail(ApiKeyExceptionCodes.MissingApiKey);
        var apiKey = await ResolveAsync(key, cancellationToken);
        if (apiKey is null)
            return ApiKeyValidationResult.Fail(ApiKeyExceptionCodes.InvalidApiKey);
        if (apiKey.ExpiresAt is not null && apiKey.ExpiresAt.Value <= _clock.Now)
            return ApiKeyValidationResult.Fail(ApiKeyExceptionCodes.ExpiredApiKey);
        return ApiKeyValidationResult.Success(apiKey);
    }

    // Anahtari once bellekte arar; yoksa repository'den tekil okumayla getirip bellege alir.
    private async Task<ApiKeyDescriptor?> ResolveAsync(string key, CancellationToken cancellationToken)
    {
        if (ResolvedApiKeys.TryGetValue(key, out var resolved))
            return resolved;
        var entity = await _apiKeyRepository.FindByKeyAsync(key, cancellationToken);
        if (entity is null)
            return null;
        var descriptor = Mapper.MapDescriptor(entity);
        ResolvedApiKeys[key] = descriptor;
        return descriptor;
    }
}
