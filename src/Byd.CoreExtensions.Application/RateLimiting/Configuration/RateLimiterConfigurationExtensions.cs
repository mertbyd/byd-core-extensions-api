using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit motorunu servis koleksiyonuna kaydeden uzanti.
/// </summary>
// islevi: Named rate limit policy'sini ve MVC convention'ini servis koleksiyonuna kaydeder.
// sistemdeki gorevi: Application modulunun rate limit motorunu tek kompozisyon noktasindan etkinlestirmesini saglar.
public static class RateLimiterConfigurationExtensions
{
    // Named policy'yi ve onu tum MVC action'larina uygulayan convention'i kaydeder.
    public static IServiceCollection AddCoreExtensionsRateLimiter(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new RateLimitConvention());
        });

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy<string, RateLimitPolicy>(RateLimitConsts.PolicyName);
        });
    }
}
