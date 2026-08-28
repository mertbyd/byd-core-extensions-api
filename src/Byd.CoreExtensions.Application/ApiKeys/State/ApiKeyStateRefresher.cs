using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Mappers.ApiKeys;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Tablodaki tum API anahtarlarini okuyup bellek durumunu tazeler.
/// </summary>
// islevi: Anahtarlarin tamamini repository ile okur, cozulmus goruntulere cevirir ve ApiKeyState.Update ile yeni versiyonlu snapshot'a yazar.
// sistemdeki gorevi: Tablo okumasini dogrulama akisinin disinda tutar; baslangicta bir kez, sonrasinda TTL doldugunda cagrilir.
// mirastan gelen:
//   ABP IReadOnlyBasicRepository<ApiKey> -> GetListAsync  (IApiKeyRepository zincirinden gelir, ayrica metot yazilmaz)
//   ABP IUnitOfWorkEnabled               -> repository cagrisi ortamda unit of work yoksa kendi kapsamini acar
// burada yazilan: RefreshAsync
public class ApiKeyStateRefresher : ITransientDependency
{
    private static readonly ApiKeyMapper Mapper = new();

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly ApiKeyState _state;
    private readonly IClock _clock;

    public ApiKeyStateRefresher(
        IApiKeyRepository apiKeyRepository,
        ApiKeyState state,
        IClock clock)
    {
        _apiKeyRepository = apiKeyRepository;
        _state = state;
        _clock = clock;
    }

    // Tablodaki tum anahtarlari okur ve anahtar degerine gore indekslenmis yeni goruntuye cevirir.
    // Ham anahtar degeri yalnizca sozluk anahtari olarak yasar; cozulmus goruntu onu tasimaz.
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _apiKeyRepository.GetListAsync(cancellationToken: cancellationToken);
        var apiKeys = new Dictionary<string, ApiKeyDescriptor>(entities.Count, StringComparer.Ordinal);
        foreach (var entity in entities)
        {
            apiKeys[entity.Key] = Mapper.MapDescriptor(entity);
        }

        _state.Update(apiKeys, _clock.Now);
    }
}
