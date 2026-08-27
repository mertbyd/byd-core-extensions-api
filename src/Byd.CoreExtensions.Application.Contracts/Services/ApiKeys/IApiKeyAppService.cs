using System;
using System.Threading.Tasks;
using Byd.CoreExtensions.Dtos.ApiKeys;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Services.ApiKeys;

/// <summary>
/// API anahtari okuma ve uretme kullanim senaryolarini tanimlar.
/// </summary>
// islevi: ApiKey tekil okuma ve uretme kontratini acar; anahtarlar guncellenmez ve silinmez.
// sistemdeki gorevi: Controller ile Application implementasyonu arasindaki public servis siniridir.
public interface IApiKeyAppService : IApplicationService
{
    // Kimligi verilen API anahtari kaydini getirir.
    Task<ApiKeyDto> GetAsync(Guid id);

    // Dogrulanmis girdiden yeni API anahtari uretir ve anahtar degerini tek seferlik doner.
    Task<CreatedApiKeyDto> CreateAsync(CreateApiKeyDto input);
}
