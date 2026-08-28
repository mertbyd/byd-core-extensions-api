using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Gelen API anahtarini bellekteki guncel goruntuye karsi dogrular.
/// </summary>
// islevi: Goruntunun tazeligini kontrol eder, gerekiyorsa tabloyu bastan okutur, ardindan anahtari arar ve son kullanma kontrolunu yapar.
// sistemdeki gorevi: Anahtar dogrulamasinin tek okuma noktasi. Goruntu tum anahtarlari tasidigi icin bilinmeyen anahtar denemesi veritabanina hic gitmez; goruntu TTL ile yenilendigi icin silinen veya suresi degistirilen anahtar en gec TTL kadar sonra dusunulur.
// mirastan gelen: yok - dogrulama akisinin sahibi oldugu icin taban sinif kullanilmaz.
// burada yazilan: ValidateAsync, GetFreshSnapshotAsync, IsExpired
public class ApiKeyStore : IApiKeyStore, ITransientDependency
{
    // Tazeleme kilidi statiktir: tazelik durumu servis ornegine degil surece aittir. TTL doldugunda
    // yalnizca tek bir istek tabloyu okur, es zamanli gelen digerleri tazelenmis sonucu bekler.
    private static readonly SemaphoreSlim RefreshGate = new(1, 1);

    private readonly IApiKeyState _state;
    private readonly ApiKeyStateRefresher _refresher;
    private readonly IClock _clock;

    public ApiKeyStore(
        IApiKeyState state,
        ApiKeyStateRefresher refresher,
        IClock clock)
    {
        _state = state;
        _refresher = refresher;
        _clock = clock;
    }

    // Gelen anahtari dogrular; ilk saglanmayan kosulun hata kodunu doner.
    public async Task<ApiKeyValidationResult> ValidateAsync(
        string? key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return ApiKeyValidationResult.Fail(ApiKeyExceptionCodes.MissingApiKey);
        var snapshot = await GetFreshSnapshotAsync(cancellationToken);
        var apiKey = snapshot.Find(key);
        if (apiKey is null)
            return ApiKeyValidationResult.Fail(ApiKeyExceptionCodes.InvalidApiKey);
        if (apiKey.ExpiresAt is not null && apiKey.ExpiresAt.Value <= _clock.Now)
            return ApiKeyValidationResult.Fail(ApiKeyExceptionCodes.ExpiredApiKey);
        return ApiKeyValidationResult.Success(apiKey);
    }

    // Goruntu tazeyse dogrudan doner; TTL dolduysa kilidi alan tek istek tabloyu yeniden yukler.
    // Kilit alindiktan sonra tazelik yeniden kontrol edilir; boylece beklerken baskasi yuklediyse
    // tablo ikinci kez okunmaz.
    private async Task<ApiKeySnapshot> GetFreshSnapshotAsync(CancellationToken cancellationToken)
    {
        if (!IsExpired(_state.Current))
            return _state.Current;
        await RefreshGate.WaitAsync(cancellationToken);
        try
        {
            if (IsExpired(_state.Current))
                await _refresher.RefreshAsync(cancellationToken);
            return _state.Current;
        }
        finally
        {
            RefreshGate.Release();
        }
    }

    // Goruntunun yuklenme zamaninin uzerinden TTL suresinin gecip gecmedigini bildirir.
    private bool IsExpired(ApiKeySnapshot snapshot)
    {
        return snapshot.LoadedAt.AddSeconds(ApiKeyConsts.CacheTtlSeconds) <= _clock.Now;
    }
}
