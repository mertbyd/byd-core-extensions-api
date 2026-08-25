using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.RateLimiting;
using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Named rate limit policy'sini MVC action endpointlerine uygular.
/// </summary>
// islevi: Her MVC selector'une ortak EnableRateLimiting metadata'sini ekler.
// sistemdeki gorevi: Controller endpointlerini named policy ile rate limit motoruna baglar.
public sealed class RateLimitConvention : IApplicationModelConvention
{
    // Tum controller action selector'lerini ortak rate limit policy'sine baglar.
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                foreach (var selector in action.Selectors)
                {
                    selector.EndpointMetadata.Add(
                        new EnableRateLimitingAttribute(RateLimitConsts.PolicyName));
                }
            }
        }
    }
}
