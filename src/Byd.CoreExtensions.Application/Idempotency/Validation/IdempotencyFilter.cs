using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Localization;
using Byd.CoreExtensions.Models.Idempotency;
using Byd.CoreExtensions.Models.ModuleSettings;
using Byd.CoreExtensions.Options;
using Byd.CoreExtensions.Providers;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Timing;
namespace Byd.CoreExtensions.Idempotency;
/// <summary>
/// Degistiren JSON isteklerinde mukerrer gonderimi dagitik cache ile engeller.
/// </summary>
// islevi: Modul ayari aciksa ve yol muaf degilse mutating JSON istegin imzasini cache'e yazar, tekrarini 409 ile keser ve basarisiz istekte imzayi temizler.
// sistemdeki gorevi: Header, attribute ve veritabani tablosu gerektirmeden uygulama genelinde kisa sureli mukerrer istek korumasi saglar.
public class IdempotencyFilter : IAsyncResourceFilter, ITransientDependency
{
    private readonly IModuleSettingRepository _moduleSettingRepository;
    private readonly CurrentModuleNameProvider _moduleNameProvider;
    private readonly IOptions<CoreExtensionsModuleOptions> _moduleOptions;
    private readonly IdempotencyRequestHasher _hasher;
    private readonly IDistributedCache<IdempotencyRecord> _cache;
    private readonly IDistributedCache<ModuleSettingCacheModel> _settingCache;
    private readonly IClock _clock;
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;
    public IdempotencyFilter(
        IModuleSettingRepository moduleSettingRepository,
        CurrentModuleNameProvider moduleNameProvider,
        IOptions<CoreExtensionsModuleOptions> moduleOptions,
        IdempotencyRequestHasher hasher,
        IDistributedCache<IdempotencyRecord> cache,
        IDistributedCache<ModuleSettingCacheModel> settingCache,
        IClock clock,
        IStringLocalizer<CoreExtensionsResource> localizer)
    {
        _moduleSettingRepository = moduleSettingRepository;
        _moduleNameProvider = moduleNameProvider;
        _moduleOptions = moduleOptions;
        _hasher = hasher;
        _cache = cache;
        _settingCache = settingCache;
        _clock = clock;
        _localizer = localizer;
    }
    // Kapsam disi istekleri dokunmadan gecirir; uygun isteklerde imzayi cache'e alip tekrari reddeder.
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!IsMutatingRequest(request.Method) || !IsEligible(request))
        {
            await next();
            return;
        }
        var cancellationToken = context.HttpContext.RequestAborted;
        var setting = await FindSettingAsync(cancellationToken);
        if (setting is null || !setting.IsEnabled || !setting.IdempotencyEnabled)
        {
            await next();
            return;
        }
        var body = await ReadBodyAsync(request);
        if (body is null)
        {
            await next();
            return;
        }
        var signature = _hasher.Compute(context.HttpContext, body);
        if (await _cache.GetAsync(signature, token: cancellationToken) is not null)
        {
            context.Result = CreateFailureResult();
            return;
        }
        await _cache.SetAsync(
            signature,
            new IdempotencyRecord { CreationTime = _clock.Now },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(setting.IdempotencyTtlSeconds)
            },
            token: cancellationToken);
        var executed = await next();
        if (IsFailedExecution(executed))
            await _cache.RemoveAsync(signature, token: cancellationToken);
    }

    // Degistiren HTTP metodu olup olmadigini bildirir.
    private static bool IsMutatingRequest(string method)
    {
        return HttpMethods.IsPost(method) ||
               HttpMethods.IsPut(method) ||
               HttpMethods.IsPatch(method) ||
               HttpMethods.IsDelete(method);
    }
    // Govdesi JSON olan, boyut sinirini asmayan ve muaf tutulmamis istekleri kapsama alir.
    private bool IsEligible(HttpRequest request)
    {
        return request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true &&
               (request.ContentLength is null || request.ContentLength <= IdempotencyConsts.MaxBodyBytes) &&
               !IsExcludedPath(request.Path);
    }

    // Yolun host tarafindan beyan edilen muafiyet on eklerinden biriyle baslayip baslamadigini bildirir.
    private bool IsExcludedPath(PathString path)
    {
        foreach (var excludedPath in _moduleOptions.Value.ExcludePaths)
        {
            if (path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
    // Govdeyi ust sinira kadar okuyup yeniden okunabilir birakir; sinir asilirsa null doner.
    private static async Task<byte[]?> ReadBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;
        using var buffer = new MemoryStream();
        var readBuffer = new byte[64 * 1024];
        try
        {
            int bytesRead;
            while ((bytesRead = await request.Body.ReadAsync(
                       readBuffer,
                       request.HttpContext.RequestAborted)) > 0)
            {
                if (buffer.Length + bytesRead > IdempotencyConsts.MaxBodyBytes)
                {
                    return null;
                }

                await buffer.WriteAsync(
                    readBuffer.AsMemory(0, bytesRead),
                    request.HttpContext.RequestAborted);
            }

            return buffer.ToArray();
        }
        finally
        {
            request.Body.Position = 0;
        }
    }
    // Ayar satirini TTL suresince cache'ten okur; sure dolunca tabloyu bir kez yeniden okur.
    private async Task<ModuleSettingCacheModel?> FindSettingAsync(CancellationToken cancellationToken)
    {
        var moduleName = _moduleNameProvider.Value;
        return await _settingCache.GetOrAddAsync(
            moduleName,
            async () => ToCacheModel(await _moduleSettingRepository.FindAsync(
                setting => setting.ModuleName == moduleName,
                cancellationToken: cancellationToken))!,
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromSeconds(ModuleSettingConsts.DefaultTtlSeconds)
            },
            token: cancellationToken);
    }
    // ModuleSetting entity'sini filtreye gereken sade cache modeline donusturur.
    private static ModuleSettingCacheModel? ToCacheModel(ModuleSetting? setting)
    {
        return setting is null
            ? null
            : new ModuleSettingCacheModel
            {
                IsEnabled = setting.IsEnabled,
                IdempotencyEnabled = setting.IdempotencyEnabled,
                IdempotencyTtlSeconds = setting.IdempotencyTtlSeconds
            };
    }
    // Calisan action'in basarisiz olup olmadigini action sonucundan veya HTTP yanit kodundan okur.
    private static bool IsFailedExecution(ResourceExecutedContext executed)
    {
        if (executed.Exception is not null)
            return true;
        var statusCode = executed.Result switch
        {
            ObjectResult result => result.StatusCode ?? executed.HttpContext.Response.StatusCode,
            IStatusCodeActionResult result => result.StatusCode ?? executed.HttpContext.Response.StatusCode,
            _ => executed.HttpContext.Response.StatusCode
        };
        return statusCode < StatusCodes.Status200OK || statusCode >= StatusCodes.Status300MultipleChoices;
    }
    // Mukerrer istek kodunu 409 durum koduyla localize ABP hata govdesine cevirir.
    private ObjectResult CreateFailureResult()
    {
        var body = new RemoteServiceErrorResponse(new RemoteServiceErrorInfo(
            _localizer[IdempotencyExceptionCodes.DuplicateRequest],
            code: IdempotencyExceptionCodes.DuplicateRequest));
        return new ObjectResult(body) { StatusCode = StatusCodes.Status409Conflict };
    }
}
