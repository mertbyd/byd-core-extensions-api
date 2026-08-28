using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Isaretli endpoint'lere gelen istegin API anahtarini dogrular ve istegi anahtarin tenant baglaminda calistirir.
/// </summary>
// islevi: Endpoint [RequireApiKey] tasiyorsa header'daki anahtari store uzerinden dogrular; gecerliyse istek baglamini doldurup tenant'i anahtara gore degistirir, degilse localize 401 ile keser.
// sistemdeki gorevi: API anahtari korumasinin tek uygulama noktasi; resource filter oldugu icin acilan tenant kapsami model binding, action ve result asamalarinin tamamini kapsar. Organizasyon kimligi kapsam acmaz, ICurrentApiKey uzerinden okunur.
public class ApiKeyValidationFilter : IAsyncResourceFilter, ITransientDependency
{
    private readonly IApiKeyStore _apiKeyStore;
    private readonly CurrentApiKey _currentApiKey;
    private readonly ICurrentTenant _currentTenant;
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;

    public ApiKeyValidationFilter(
        IApiKeyStore apiKeyStore,
        CurrentApiKey currentApiKey,
        ICurrentTenant currentTenant,
        IStringLocalizer<CoreExtensionsResource> localizer)
    {
        _apiKeyStore = apiKeyStore;
        _currentApiKey = currentApiKey;
        _currentTenant = currentTenant;
        _localizer = localizer;
    }

    // Endpoint anahtar gerektirmiyorsa dokunmadan gecer; gerektiriyorsa dogrular ve istegin kalanini anahtarin tenant baglaminda yurutur.
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<RequireApiKeyAttribute>() is null)
        {
            await next();
            return;
        }
        var result = await _apiKeyStore.ValidateAsync(
            context.HttpContext.Request.Headers[ApiKeyConsts.HeaderName].ToString(),
            context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            context.Result = CreateFailureResult(result.ErrorCode!);
            return;
        }
        var apiKey = result.ApiKey!;
        _currentApiKey.Set(apiKey);
        // Anahtar hangi tenant'a aitse istegin kalani o tenant olarak calisir; tenant bos ise baglam host kalir.
        using (_currentTenant.Change(apiKey.TenantId))
        {
            await next();
        }
    }

    // Basarisizlik kodunu 401 durum koduyla localize ABP hata govdesine cevirir.
    private ObjectResult CreateFailureResult(string errorCode)
    {
        var body = new RemoteServiceErrorResponse(new RemoteServiceErrorInfo(
            _localizer[errorCode],
            code: errorCode));
        return new ObjectResult(body) { StatusCode = StatusCodes.Status401Unauthorized };
    }
}
