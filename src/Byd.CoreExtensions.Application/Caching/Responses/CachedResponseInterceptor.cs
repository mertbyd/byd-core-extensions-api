using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Etos.Caching;
using Byd.CoreExtensions.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Json;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// [CachedResponse] tasiyan servis metotlarinin sonucunu Redis'ten doner ya da calistirip saklar; [ClearCachedResponse] tasiyan
/// metottan sonra grubu temizletir.
/// </summary>
// islevi: Isaretli metotta yanit onbellegi aciksa cagrinin ozetini kurar; kayit varsa metodu calistirmadan onu doner, yoksa
// metodu calistirir ve sonucu suresiyle yazar. Ozet kulturden, metottan, girdilerden ve istenmisse kullanicidan kurulur;
// tenant ve organizasyonu depo ekler. Temizlik isaretli metot hatasiz bitince grubun silinmesi commit sonrasina birakilir.
// sistemdeki gorevi: Yanit onbelleginin tek uygulama noktasi; isaretsiz metotlara ve Redis kapali host'a dokunmaz. Servisin
// icinden kendi metoduna yapilan cagri proxy'den gecmedigi icin onbellege girmez.
public class CachedResponseInterceptor(
    CachedResponseStore store,
    ISettingProvider settingProvider,
    ICurrentUser currentUser,
    IJsonSerializer jsonSerializer,
    ILocalEventBus localEventBus,
    ICurrentTenant currentTenant)
    : AbpInterceptor, ITransientDependency
{
    // Temizlik isaretliyse metodu calistirip grubu temizletir; okuma isaretliyse kaydi doner ya da metodu calistirip saklar.
    public override async Task InterceptAsync(IAbpMethodInvocation invocation)
    {
        var clearing = FindAttribute<ClearCachedResponseAttribute>(invocation);
        var caching = FindAttribute<CachedResponseAttribute>(invocation);
        await (clearing is not null ? ProceedAndClearAsync(invocation, clearing)
            : caching is not null && ReturnsTaskWithResult(invocation) && await IsEnabledAsync()
                ? ServeOrStoreAsync(invocation, caching)
                : invocation.ProceedAsync());
    }

    // Metot hatasiz biterse grubun temizligini yururlukteki tenant'la commit sonrasina birakir; islem yoksa hemen temizlenir.
    private async Task ProceedAndClearAsync(
        IAbpMethodInvocation invocation, ClearCachedResponseAttribute attribute, CancellationToken cancellationToken = default)
    {
        await invocation.ProceedAsync();
        await localEventBus.PublishAsync(new CachedResponseSourceChangedEto(attribute.Group, currentTenant.Id));
    }

    // Kayit varsa metodu calistirmadan onu doner; yoksa metodu calistirip sonucu suresiyle saklar.
    private async Task ServeOrStoreAsync(
        IAbpMethodInvocation invocation, CachedResponseAttribute attribute, CancellationToken cancellationToken = default)
    {
        var fingerprint = CreateFingerprint(invocation, attribute);
        var cached = await store.FindAsync(attribute.Group, fingerprint, cancellationToken);
        if (cached is not null)
        {
            invocation.ReturnValue = jsonSerializer.Deserialize(invocation.Method.ReturnType.GenericTypeArguments[0], cached);
            return;
        }
        await invocation.ProceedAsync();
        if (invocation.ReturnValue is { } value)
            await store.SetAsync(attribute.Group, fingerprint, jsonSerializer.Serialize(value),
                TimeSpan.FromMinutes(attribute.DurationMinutes), cancellationToken);
    }

    // Metodun ya da ezdigi taban metodun tasidigi isareti getirir.
    private static TAttribute? FindAttribute<TAttribute>(IAbpMethodInvocation invocation)
        where TAttribute : Attribute
        => invocation.Method.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>().FirstOrDefault();

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
