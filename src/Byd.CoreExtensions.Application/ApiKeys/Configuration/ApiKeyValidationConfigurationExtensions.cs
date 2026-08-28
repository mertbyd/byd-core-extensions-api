using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// API anahtari dogrulamasini servis koleksiyonuna kaydeden uzanti.
/// </summary>
// islevi: Dogrulama filtresini global MVC filtresi olarak kaydeder.
// sistemdeki gorevi: Application modulunun API anahtari korumasini tek kompozisyon noktasindan etkinlestirmesini saglar; host tarafinda kablolama gerekmez.
public static class ApiKeyValidationConfigurationExtensions
{
    // Dogrulama filtresini tum MVC action'larina uygulanacak sekilde kaydeder.
    public static IServiceCollection AddCoreExtensionsApiKeyValidation(this IServiceCollection services)
    {
        return services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ApiKeyValidationFilter>();
        });
    }
}
