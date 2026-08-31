using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
namespace Byd.CoreExtensions.Idempotency;
/// <summary>
/// Anti-duplicate filtresini servis koleksiyonuna kaydeden uzanti.
/// </summary>
// islevi: Mukerrer istek filtresini global MVC filtresi olarak kaydeder.
// sistemdeki gorevi: Application modulunun anti-duplicate korumasini tek kompozisyon noktasindan etkinlestirmesini saglar; host tarafinda kablolama gerekmez. Ic tiplerin adi Idempotency kalir, disariya acilan kompozisyon adi AddAntiDuplicate'tir.
public static class IdempotencyConfigurationExtensions
{
    // Anti-duplicate filtresini tum MVC action'larina uygulanacak sekilde kaydeder.
    public static IServiceCollection AddAntiDuplicate(this IServiceCollection services)
    {
        return services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<IdempotencyFilter>();
        });
    }
}
