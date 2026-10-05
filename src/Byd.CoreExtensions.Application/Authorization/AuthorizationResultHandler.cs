using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Http;
using Volo.Abp.Json;

namespace Byd.CoreExtensions.Authorization;

/// <summary>
/// Yetki reddini sebebiyle birlikte ABP hata govdesine yazar.
/// </summary>
// islevi: Reddedilen iznin bagli oldugu feature kapsamda kapaliysa feature mesajini, degilse yetki mesajini 403 ile dondurur.
// sistemdeki gorevi: Controller'daki izin reddinin bos 403 yerine localize hata koduyla donmesini saglar; giris isteyen ve
// basarili sonuclari cercevenin varsayilan isleyicisine birakir.
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAuthorizationMiddlewareResultHandler))]
public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler, ITransientDependency
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IFeatureDefinitionManager _featureDefinitionManager;
    private readonly IFeatureChecker _featureChecker;
    private readonly IStringLocalizer<CoreExtensionsResource> _localizer;
    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly IJsonSerializer _serializer;

    public AuthorizationResultHandler(
        IPermissionDefinitionManager permissionDefinitionManager,
        IFeatureDefinitionManager featureDefinitionManager,
        IFeatureChecker featureChecker,
        IStringLocalizer<CoreExtensionsResource> localizer,
        IStringLocalizerFactory localizerFactory,
        IJsonSerializer serializer)
    {
        _permissionDefinitionManager = permissionDefinitionManager;
        _featureDefinitionManager = featureDefinitionManager;
        _featureChecker = featureChecker;
        _localizer = localizer;
        _localizerFactory = localizerFactory;
        _serializer = serializer;
    }

    // Yalniz giris yapmis kullanicinin yetki reddini yazar; diger sonuclarda cercevenin davranisi korunur.
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Forbidden)
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var cancellationToken = context.RequestAborted;
        var featureName = await FindDisabledFeatureAsync(authorizeResult.AuthorizationFailure, cancellationToken);
        var error = featureName is null
            ? CreateError(GeneralExceptionCodes.PermissionNotGranted)
            : CreateError(GeneralExceptionCodes.FeatureNotEnabled, await GetDisplayNameAsync(featureName, cancellationToken));
        await WriteForbiddenAsync(context, error, cancellationToken);
    }

    // Reddedilen izinlerin bagli oldugu feature'lardan kapsamda kapali olan ilkini bulur.
    private async Task<string?> FindDisabledFeatureAsync(
        AuthorizationFailure? failure,
        CancellationToken cancellationToken = default)
    {
        foreach (var featureName in await GetRequiredFeaturesAsync(failure, cancellationToken))
        {
            if (!await _featureChecker.IsEnabledAsync(featureName))
                return featureName;
        }

        return null;
    }

    // Reddedilen izinlerin tanimlarindan bagli feature adlarini toplar.
    private async Task<List<string>> GetRequiredFeaturesAsync(
        AuthorizationFailure? failure,
        CancellationToken cancellationToken = default)
    {
        var featureNames = new List<string>();
        foreach (var permissionName in GetPermissionNames(failure))
        {
            var permission = await _permissionDefinitionManager.GetOrNullAsync(permissionName);
            if (permission is not null)
                featureNames.AddRange(permission.StateCheckers
                    .OfType<RequireFeaturesSimpleStateChecker<PermissionDefinition>>()
                    .SelectMany(checker => checker.FeatureNames));
        }

        return featureNames.Distinct().ToList();
    }

    // Cercevenin tekli ve coklu izin requirement'larindaki izin adlarini okur.
    private static IEnumerable<string> GetPermissionNames(AuthorizationFailure? failure)
        => failure?.FailedRequirements.SelectMany(requirement => requirement switch
        {
            PermissionRequirement single => new[] { single.PermissionName },
            PermissionsRequirement multiple => multiple.PermissionNames,
            _ => Array.Empty<string>()
        }) ?? Enumerable.Empty<string>();

    // Feature'in gosterim adini istegin dilinde dondurur; tanim bulunamazsa adini kullanir.
    private async Task<string> GetDisplayNameAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var feature = await _featureDefinitionManager.GetOrNullAsync(featureName);
        return feature?.DisplayName.Localize(_localizerFactory).Value ?? featureName;
    }

    // Hata kodunun localize mesajini ABP hata bilgisine cevirir.
    private RemoteServiceErrorInfo CreateError(string code, params object[] arguments)
        => new(_localizer[code, arguments], code: code);

    // 403 durum kodunu ve ABP hata govdesini yazar.
    private async Task WriteForbiddenAsync(
        HttpContext context,
        RemoteServiceErrorInfo error,
        CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsync(_serializer.Serialize(new RemoteServiceErrorResponse(error)), cancellationToken);
    }
}
