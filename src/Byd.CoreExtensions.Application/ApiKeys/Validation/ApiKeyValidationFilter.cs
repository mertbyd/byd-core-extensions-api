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
/// Isaretli endpoint'lere gelen istegin API anahtarini dogrular ve istegi anahtarin kapsaminda calistirir.
/// </summary>
// islevi: Endpoint [RequireApiKey] tasiyorsa header'daki anahtari store uzerinden dogrular; gecerliyse istek baglamini doldurup tenant ve organizasyon kapsamini anahtara gore degistirir, degilse localize 401 ile keser.
// sistemdeki gorevi: API anahtari korumasinin tek uygulama noktasi; resource filter oldugu icin acilan kapsamlar model binding, action ve result asamalarinin tamamini kapsar.
public class ApiKeyValidationFilter : IAsyncResourceFilter, ITransientDependency
{
    private readonly IApiKeyStore _apiKeyStore;
    private readonly CurrentApiKey _currentApiKey;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentOrganization _currentOrganization;
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;

    public ApiKeyValidationFilter(
        IApiKeyStore apiKeyStore,
        CurrentApiKey currentApiKey,
        ICurrentTenant currentTenant,
        ICurrentOrganization currentOrganization,
        IStringLocalizer<CoreExtensionsResource> localizer)
    {
        _apiKeyStore = apiKeyStore;
        _currentApiKey = currentApiKey;
        _currentTenant = currentTenant;
        _currentOrganization = currentOrganization;
        _localizer = localizer;
    }

    // Endpoint anahtar gerektirmiyorsa dokunmadan gecer; gerektiriyorsa dogrular ve istegin kalanini anahtarin kapsaminda yurutur.
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
        // Istegin kalani anahtarin tenant ve organizasyon kapsaminda calisir; alanlar bos ise kapsam acilmamis
        // sayilir. Anahtarin kendi kaydi bu filtreye takilmaz: anahtar istege kapsam bilgisi olmadan gelir ve
        // kapsami zaten kendisi belirler.
        using (_currentTenant.Change(apiKey.TenantId))
        using (_currentOrganization.Change(apiKey.OrganizationId))
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
