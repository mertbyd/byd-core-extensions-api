using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// [CachedResponse] tasiyan action'larin yanitini Redis'ten doner ya da calistirip saklar.
/// </summary>
// islevi: Isaretli action'da yanit onbellegi aciksa istegin ozetini kurar; kayit varsa action'i calistirmadan onu doner, yoksa
// action'i calistirir ve basarili sonucu suresiyle yazar. Ozet kulturden, yoldan, action girdilerinden ve istenmisse kullanicidan
// kurulur; tenant ve organizasyonu depo ekler.
// sistemdeki gorevi: Yanit onbelleginin tek uygulama noktasi; isaretsiz action'lara ve Redis kapali host'a dokunmaz.
public class CachedResponseFilter(
    CachedResponseStore store,
    ISettingProvider settingProvider,
    ICurrentUser currentUser,
    IOptions<JsonOptions> jsonOptions)
    : IAsyncActionFilter, ITransientDependency
{
    // Kayit varsa onu doner; yoksa action'i calistirip basarili sonucu saklar.
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<CachedResponseAttribute>();
        var cancellationToken = context.HttpContext.RequestAborted;
        if (attribute is null || !await IsEnabledAsync(cancellationToken))
        {
            await next();
            return;
        }
        var fingerprint = CreateFingerprint(context, attribute);
        var cached = await store.FindAsync(attribute.Group, fingerprint, cancellationToken);
        if (cached is not null)
        {
            context.HttpContext.Response.Headers[CachedResponseConsts.CacheHeaderName] = CachedResponseConsts.CacheHitValue;
            context.Result = new ContentResult { Content = cached, ContentType = MimeTypes.Application.Json, StatusCode = StatusCodes.Status200OK };
            return;
        }
        var executed = await next();
        if (executed.Exception is null && executed.Result is ObjectResult { Value: { } value, StatusCode: null or StatusCodes.Status200OK })
            await store.SetAsync(
                attribute.Group,
                fingerprint,
                Serialize(value),
                TimeSpan.FromMinutes(attribute.DurationMinutes),
                cancellationToken);
    }

    // Redis bagli ve yanit onbellegi ayarlarda aciksa true doner.
    private async Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
    {
        return store.IsAvailable
               && await settingProvider.IsTrueAsync(CoreExtensionsSettings.Caching.IsResponseCachingEnabled);
    }

    // Kultur, metot, yol, action girdileri ve istenmisse kullanicidan tek SHA256 ozeti kurar.
    private string CreateFingerprint(ActionExecutingContext context, CachedResponseAttribute attribute)
    {
        var arguments = context.ActionArguments
            .Where(argument => argument.Value is not CancellationToken)
            .OrderBy(argument => argument.Key, StringComparer.Ordinal)
            .ToDictionary(argument => argument.Key, argument => argument.Value);
        var payload = string.Join(
            CachedResponseConsts.KeySeparator,
            CultureInfo.CurrentUICulture.Name,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value,
            attribute.IsUserScoped ? currentUser.Id?.ToString() ?? CachedResponseConsts.NoScope : CachedResponseConsts.NoScope,
            Serialize(arguments));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    // Degeri MVC'nin yaniti yazarken kullandigi JSON secenekleriyle metne cevirir.
    private string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, value.GetType(), jsonOptions.Value.JsonSerializerOptions);
    }
}
