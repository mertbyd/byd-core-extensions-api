using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.Json;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// [CachedResponse] tasiyan servis metotlarinin sonucunu Redis'ten doner ya da calistirip saklar.
/// </summary>
// islevi: Isaretli metotta yanit onbellegi aciksa cagrinin ozetini kurar; kayit varsa metodu calistirmadan onu doner, yoksa
// metodu calistirir ve sonucu suresiyle yazar. Ozet kulturden, metottan, girdilerden ve istenmisse kullanicidan kurulur;
// tenant ve organizasyonu depo ekler.
// sistemdeki gorevi: Yanit onbelleginin tek uygulama noktasi; isaretsiz metotlara ve Redis kapali host'a dokunmaz. Servisin
// icinden kendi metoduna yapilan cagri proxy'den gecmedigi icin onbellege girmez.
public class CachedResponseInterceptor(
    CachedResponseStore store,
    ISettingProvider settingProvider,
    ICurrentUser currentUser,
    IJsonSerializer jsonSerializer)
    : AbpInterceptor, ITransientDependency
{
    // Kayit varsa onu doner; yoksa metodu calistirip sonucu saklar.
    public override async Task InterceptAsync(IAbpMethodInvocation invocation)
    {
        var attribute = invocation.Method.GetCustomAttributes(typeof(CachedResponseAttribute), true)
            .OfType<CachedResponseAttribute>().FirstOrDefault();
        if (attribute is null || !ReturnsTaskWithResult(invocation) || !await IsEnabledAsync())
        {
            await invocation.ProceedAsync();
            return;
        }
        var fingerprint = CreateFingerprint(invocation, attribute);
        var cached = await store.FindAsync(attribute.Group, fingerprint);
        if (cached is not null)
        {
            invocation.ReturnValue = jsonSerializer.Deserialize(invocation.Method.ReturnType.GenericTypeArguments[0], cached);
            return;
        }
        await invocation.ProceedAsync();
        if (invocation.ReturnValue is { } value)
            await store.SetAsync(attribute.Group, fingerprint, jsonSerializer.Serialize(value),
                TimeSpan.FromMinutes(attribute.DurationMinutes));
    }

    // Yalniz sonuc donduren asenkron metotlar saklanir; sonucsuz ya da senkron metot isaretli olsa da calisir.
    private static bool ReturnsTaskWithResult(IAbpMethodInvocation invocation)
    {
        return invocation.Method.ReturnType.IsGenericType
               && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
    }

    // Yanit onbellegi ayarla acik ve Redis bagliysa true doner.
    private async Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
    {
        return store.IsAvailable
               && await settingProvider.IsTrueAsync(CoreExtensionsSettings.Caching.IsResponseCachingEnabled);
    }

    // Kultur, metodun tam adi, istenmisse kullanici ve iptal belirteci disindaki girdilerden ozet kurar.
    private string CreateFingerprint(IAbpMethodInvocation invocation, CachedResponseAttribute attribute)
    {
        var arguments = invocation.ArgumentsDictionary
            .Where(argument => argument.Value is not CancellationToken)
            .ToDictionary(argument => argument.Key, argument => argument.Value);
        var payload = string.Join(
            CachedResponseConsts.KeySeparator,
            CultureInfo.CurrentUICulture.Name,
            invocation.Method.DeclaringType?.FullName,
            invocation.Method.Name,
            attribute.IsUserScoped ? currentUser.Id?.ToString() ?? CachedResponseConsts.NoScope : CachedResponseConsts.NoScope,
            jsonSerializer.Serialize(arguments));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
