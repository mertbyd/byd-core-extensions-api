using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// Yanit onbellegini servis koleksiyonuna kaydeden uzanti.
/// </summary>
// islevi: Yanit onbellegi filtresini global MVC filtresi olarak kaydeder.
// sistemdeki gorevi: Application modulunun yanit onbellegini tek kompozisyon noktasindan etkinlestirmesini saglar; host tarafinda
// kablolama gerekmez, action'lar yalniz [CachedResponse] ile isaretlenir.
public static class CachedResponseConfigurationExtensions
{
    // Yanit onbellegi filtresini tum MVC action'larina uygulanacak sekilde kaydeder.
    public static IServiceCollection AddCoreExtensionsCachedResponses(this IServiceCollection services)
    {
        return services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<CachedResponseFilter>();
        });
    }
}
