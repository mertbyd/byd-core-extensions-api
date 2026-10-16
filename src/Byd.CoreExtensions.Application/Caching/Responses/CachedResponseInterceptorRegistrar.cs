using System.Linq;
using Byd.CoreExtensions.Attributes;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// [CachedResponse] tasiyan metodu olan servislere yanit onbellegi interceptor'ini ekler.
/// </summary>
// islevi: Kaydedilen her servisin metotlarina bakar; isaretli metot varsa servisi onbellek interceptor'iyla proxy'ler.
// sistemdeki gorevi: Isaretsiz servislerin proxy maliyetine girmemesini saglar; modul acilisinda bir kez baglanir.
public static class CachedResponseInterceptorRegistrar
{
    // Isaretli metodu olan servise interceptor'i ekler.
    public static void RegisterIfNeeded(IOnServiceRegistredContext context)
    {
        if (context.ImplementationType.GetMethods().Any(method => method.IsDefined(typeof(CachedResponseAttribute), true)))
            context.Interceptors.TryAdd<CachedResponseInterceptor>();
    }
}
