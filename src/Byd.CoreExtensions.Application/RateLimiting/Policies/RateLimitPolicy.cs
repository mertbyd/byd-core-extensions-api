using System;
using System.Globalization;
using System.Net.Mime;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Localization;
using Volo.Abp.Http;
using Volo.Abp.Json;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Endpoint rate limit kuralini ASP.NET Core named policy olarak uygular.
/// </summary>
// islevi: Endpoint metadata'sini ve guncel snapshot'i okuyup istege uygun partition limiter'ini uretir.
// sistemdeki gorevi: Rate limit algoritmasi, partition anahtari ve localize 429 yanitinin tek policy sahibidir.
public sealed class RateLimitPolicy : IRateLimiterPolicy<string>
{
    private readonly IRateLimitState _state;
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;
    private readonly IJsonSerializer _serializer;

    public RateLimitPolicy(
        IRateLimitState state,
        IStringLocalizer<CoreExtensionsResource> localizer,
        IJsonSerializer serializer)
    {
        _state = state;
        _localizer = localizer;
        _serializer = serializer;
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => WriteRejectionAsync;

    // Endpoint kuralini ve guncel snapshot'i okuyup istege uygun partition limiter'ini kurar.
    public RateLimitPartition<string> GetPartition(HttpContext context)
    {
        var rule = context.GetEndpoint()?.Metadata.GetMetadata<RateLimitAttribute>();
        // Policy singleton olarak etkinlestirilir; scoped ABP kimlik servislerini resolver request scope'undan alir.
        var resolver = context.RequestServices.GetRequiredService<RateLimitPartitionResolver>();
        var snapshot = _state.Current;
        var algorithm = ResolveAlgorithm(rule, snapshot);
        var permitLimit = ResolvePermitLimit(rule, snapshot);
        var windowSeconds = ResolveWindowSeconds(rule, snapshot);
        var queueLimit = rule?.QueueLimit ?? RateLimitConsts.DefaultQueueLimit;
        var segmentsPerWindow = ResolveSegmentsPerWindow(rule);
        var tokensPerPeriod = ResolveTokensPerPeriod(rule, permitLimit);
        return RateLimitPartitionFactory.Create(
            algorithm,
            BuildPartitionKey(
                resolver.Resolve(context, rule),
                algorithm,
                permitLimit,
                windowSeconds,
                queueLimit,
                segmentsPerWindow,
                tokensPerPeriod,
                snapshot.Version),
            permitLimit,
            windowSeconds,
            queueLimit,
            segmentsPerWindow,
            tokensPerPeriod);
    }

    // Partition anahtarini kimlik, cozulmus kural imzasi ve snapshot versiyonundan kurar.
    // Limiter'lar anahtar basina onbelleklenir; imza olmadan ayni kimlikle vurulan farkli kurallar
    // ilk endpointin limiter'ini paylasir ve kendi oznitelik degerlerini kaybeder.
    private static string BuildPartitionKey(
        string identity,
        string algorithm,
        int permitLimit,
        int windowSeconds,
        int queueLimit,
        int segmentsPerWindow,
        int tokensPerPeriod,
        long version)
    {
        return string.Join(
            RateLimitConsts.PartitionKeySeparator,
            identity,
            algorithm,
            permitLimit.ToString(CultureInfo.InvariantCulture),
            windowSeconds.ToString(CultureInfo.InvariantCulture),
            queueLimit.ToString(CultureInfo.InvariantCulture),
            segmentsPerWindow.ToString(CultureInfo.InvariantCulture),
            tokensPerPeriod.ToString(CultureInfo.InvariantCulture),
            version.ToString(CultureInfo.InvariantCulture));
    }

    // Oznitelik algoritma belirtmediyse snapshot'taki global algoritmayi kullanir.
    private static string ResolveAlgorithm(RateLimitAttribute? rule, RateLimitSnapshot snapshot)
    {
        return string.IsNullOrEmpty(rule?.Algorithm) ? snapshot.Algorithm : rule.Algorithm;
    }
    // Oznitelik limit belirtmediyse snapshot'taki global limiti kullanir.
    private static int ResolvePermitLimit(RateLimitAttribute? rule, RateLimitSnapshot snapshot)
    {
        return rule is { PermitLimit: > 0 } ? rule.PermitLimit : snapshot.PermitLimit;
    }
    // Oznitelik pencere belirtmediyse snapshot'taki global pencereyi kullanir.
    private static int ResolveWindowSeconds(RateLimitAttribute? rule, RateLimitSnapshot snapshot)
    {
        return rule is { WindowSeconds: > 0 } ? rule.WindowSeconds : snapshot.WindowSeconds;
    }

    // Oznitelik dilim sayisi belirtmediyse sabit varsayilani kullanir.
    private static int ResolveSegmentsPerWindow(RateLimitAttribute? rule)
    {
        return rule is { SegmentsPerWindow: > 0 }
            ? rule.SegmentsPerWindow
            : RateLimitConsts.DefaultSegmentsPerWindow;
    }

    // Oznitelik periyot basina token belirtmediyse limit kadarini kullanir.
    private static int ResolveTokensPerPeriod(RateLimitAttribute? rule, int permitLimit)
    {
        return rule is { TokensPerPeriod: > 0 } ? rule.TokensPerPeriod : permitLimit;
    }

    // 429 durum kodunu, Retry-After basligini ve ABP hata govdesini yazar.
    private async ValueTask WriteRejectionAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }
        var body = new RemoteServiceErrorResponse(new RemoteServiceErrorInfo(
            _localizer[GeneralExceptionCodes.TooManyRequests],
            code: GeneralExceptionCodes.TooManyRequests));
        response.ContentType = MediaTypeNames.Application.Json;
        await response.WriteAsync(_serializer.Serialize(body), cancellationToken);
    }
}
