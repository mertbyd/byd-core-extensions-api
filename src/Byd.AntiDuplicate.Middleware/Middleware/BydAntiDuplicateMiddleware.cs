using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Byd.AntiDuplicate.Middleware.Hashing;
using Byd.CoreExtensions.AntiDuplicate;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Localization;
using Byd.CoreExtensions.ModuleSettings;
using Byd.CoreExtensions.Options;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Timing;
namespace Byd.AntiDuplicate.Middleware.Middleware;
/// <summary>
/// Degistiren JSON isteklerinde mukerrer gonderimi dagitik cache ile engeller.
/// </summary>
// islevi: Istegin kapsamina ait modul ayari aciksa ve istek muaf degilse mutating JSON istegin imzasini cache'e yazar, tekrarini 409 ile keser ve basarisiz istekte imzayi temizler.
// sistemdeki gorevi: Header, attribute ve veritabani tablosu gerektirmeden uygulama genelinde kisa sureli mukerrer istek korumasi saglar.
// IMiddleware secildi: konvansiyonel RequestDelegate imzasi middleware'i singleton yapar ve istek basina degisen
// bagimliliklarin alan olarak tutulmasini imkansiz kilar. IMiddleware ile ABP kaydi istek kapsaminda cozulur.
public class BydAntiDuplicateMiddleware : IMiddleware, ITransientDependency
{
    private readonly IModuleSettingProvider _moduleSettingProvider;
    private readonly IOptions<CoreExtensionsModuleOptions> _moduleOptions;
    private readonly IdempotencyRequestHasher _hasher;
    private readonly IDistributedCache<IdempotencyRecord> _cache;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IClock _clock;
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;
    public BydAntiDuplicateMiddleware(
        IModuleSettingProvider moduleSettingProvider,
        IOptions<CoreExtensionsModuleOptions> moduleOptions,
        IdempotencyRequestHasher hasher,
        IDistributedCache<IdempotencyRecord> cache,
        IJsonSerializer jsonSerializer,
        IClock clock,
        IStringLocalizer<CoreExtensionsResource> localizer)
    {
        _moduleSettingProvider = moduleSettingProvider;
        _moduleOptions = moduleOptions;
        _hasher = hasher;
        _cache = cache;
        _jsonSerializer = jsonSerializer;
        _clock = clock;
        _localizer = localizer;
    }
    // Kapsam disi istekleri dokunmadan gecirir; uygun isteklerde imzayi cache'e alip tekrari reddeder.
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cancellationToken = context.RequestAborted;
        var setting = await FindApplicableSettingAsync(context, cancellationToken);
        if (setting is null)
        {
            await next(context);
            return;
        }
        var body = await ReadBodyAsync(context.Request, cancellationToken);
        if (body is null)
        {
            await next(context);
            return;
        }
        var signature = _hasher.Compute(context, body);
        if (await _cache.GetAsync(signature, token: cancellationToken) is not null)
        {
            await WriteDuplicateResponseAsync(context, cancellationToken);
            return;
        }
        await ReserveSignatureAsync(signature, setting.IdempotencyTtlSeconds, cancellationToken);
        try
        {
            await next(context);
        }
        catch
        {
            // Sonraki halka patlarsa rezervasyon kalmamali; istemci ayni istegi tekrar deneyebilsin diye
            // imza silinir ve hata degistirilmeden ust katmana birakilir.
            await _cache.RemoveAsync(signature, token: cancellationToken);
            throw;
        }
        if (IsFailedResponse(context.Response.StatusCode))
            await _cache.RemoveAsync(signature, token: cancellationToken);
    }

    // Istegin koruma kapsaminda olup olmadigini ve modul ayarinin acik olup olmadigini tek karara indirger.
    // Kapsam disi veya kapali her durumda null doner; cagiran taraf tek kosulla akisi devam ettirir.
    private async Task<ModuleSettingCacheModel?> FindApplicableSettingAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var request = context.Request;
        if (!IsMutatingRequest(request.Method) || !IsEligible(request))
        {
            return null;
        }
        var setting = await _moduleSettingProvider.FindCurrentAsync(cancellationToken);
        return setting is not null && setting.IsEnabled && setting.IdempotencyEnabled
            ? setting
            : null;
    }

    // Imzayi ayardan gelen tekrar penceresi boyunca rezerve eder.
    private async Task ReserveSignatureAsync(string signature, int ttlSeconds, CancellationToken cancellationToken = default)
    {
        await _cache.SetAsync(
            signature,
            new IdempotencyRecord { CreationTime = _clock.Now },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds)
            },
            token: cancellationToken);
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
        return request.ContentType?.Contains(IdempotencyConsts.JsonContentType, StringComparison.OrdinalIgnoreCase) == true &&
               (request.ContentLength is null || request.ContentLength <= IdempotencyConsts.MaxBodyBytes) &&
               !IsExcluded(request);
    }

    // Istegin host tarafindan beyan edilen muafiyetlerden birine girip girmedigini bildirir.
    // Bir beyan ya bir HTTP metodudur ("/get" tum GET istekleri) ya da bir yol on ekidir
    // ("/api/orders" o alt agacin tamami). Once metot denenir, tutmazsa yol denenir.
    private bool IsExcluded(HttpRequest request)
    {
        foreach (var exclusion in _moduleOptions.Value.ExcludePaths)
        {
            if (MatchesMethod(exclusion, request.Method) || MatchesPath(exclusion, request.Path))
            {
                return true;
            }
        }

        return false;
    }

    // Beyani bastaki ayiractan arindirip istegin HTTP metoduyla karsilastirir.
    // Metot adi listesi tutulmaz; karsilastirma dogrudan istegin metoduna yapildigi icin
    // GET, POST, PATCH ve ileride eklenecek her metot ayrica kod degisikligi olmadan calisir.
    private static bool MatchesMethod(string exclusion, string method)
    {
        return string.Equals(exclusion.TrimStart('/'), method, StringComparison.OrdinalIgnoreCase);
    }

    // Beyani yol on eki olarak dener. PathString cevrimi bastaki ayiraci zorunlu tuttugu icin
    // ayiracla baslamayan beyanlar yol olarak degerlendirilmez; boyle bir beyan yalnizca metot olabilir.
    private static bool MatchesPath(string exclusion, PathString path)
    {
        return exclusion.StartsWith('/') &&
               path.StartsWithSegments(exclusion, StringComparison.OrdinalIgnoreCase);
    }
    // Govdeyi ust sinira kadar okuyup yeniden okunabilir birakir; sinir asilirsa null doner.
    private static async Task<byte[]?> ReadBodyAsync(
        HttpRequest request,
        CancellationToken cancellationToken = default)
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
                       cancellationToken)) > 0)
            {
                if (buffer.Length + bytesRead > IdempotencyConsts.MaxBodyBytes)
                {
                    return null;
                }

                await buffer.WriteAsync(
                    readBuffer.AsMemory(0, bytesRead),
                    cancellationToken);
            }

            return buffer.ToArray();
        }
        finally
        {
            request.Body.Position = 0;
        }
    }
    // Yanitin basarisiz olup olmadigini durum kodundan okur. Filtre halindeki tipli action sonucu
    // artik yok; middleware'in gorebildigi tek isaret HTTP durum kodudur.
    private static bool IsFailedResponse(int statusCode)
    {
        return statusCode < StatusCodes.Status200OK || statusCode >= StatusCodes.Status300MultipleChoices;
    }
    // Mukerrer istek kodunu 409 durum koduyla localize ABP hata govdesine cevirip yanita yazar.
    private async Task WriteDuplicateResponseAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var body = new RemoteServiceErrorResponse(new RemoteServiceErrorInfo(
            _localizer[IdempotencyExceptionCodes.DuplicateRequest],
            code: IdempotencyExceptionCodes.DuplicateRequest));
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        context.Response.ContentType = IdempotencyConsts.JsonContentType;
        await context.Response.WriteAsync(_jsonSerializer.Serialize(body), cancellationToken);
    }
}
