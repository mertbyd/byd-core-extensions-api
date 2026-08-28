using Microsoft.AspNetCore.Http;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Clients;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Bir istegin rate limit sayacini hangi kimlige gore bolecegini cozer.
/// </summary>
// islevi: Endpoint'in partition stratejisine gore istekten kimlik cikarir; kimlik yoksa daima IP'ye duser.
// sistemdeki gorevi: Limiter policy'nin partition anahtarini uretir; her strateji istekleri farkli sayac bolmelerine ayirir.
public class RateLimitPartitionResolver : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentClient _currentClient;

    public RateLimitPartitionResolver(
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        ICurrentClient currentClient)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _currentClient = currentClient;
    }

    // Endpoint stratejisine gore partition anahtarini uretir; strateji yoksa veya kimlik bulunamazsa IP'ye duser.
    public string Resolve(HttpContext context, RateLimitAttribute? rule)
    {
        return rule?.PartitionStrategy switch
        {
            RateLimitPartitionStrategy.User => UserKey(context),
            RateLimitPartitionStrategy.Tenant => TenantKey(context),
            RateLimitPartitionStrategy.Client => ClientKey(context),
            RateLimitPartitionStrategy.UserAndEndpoint => UserAndEndpointKey(context),
            RateLimitPartitionStrategy.TenantAndUser => TenantAndUserKey(context),
            _ => IpKey(context)
        };
    }

    // Kullanici kimligine gore anahtar; oturum yoksa IP'ye duser.
    private string UserKey(HttpContext context)
    {
        return _currentUser.Id is { } userId
            ? Key(RateLimitPartitionStrategy.User, userId.ToString())
            : IpKey(context);
    }

    // Tenant kimligine gore anahtar; istek host tarafinda ise IP'ye duser.
    private string TenantKey(HttpContext context)
    {
        return _currentTenant.Id is { } tenantId
            ? Key(RateLimitPartitionStrategy.Tenant, tenantId.ToString())
            : IpKey(context);
    }

    // Client kimligine gore anahtar; client yoksa IP'ye duser.
    private string ClientKey(HttpContext context)
    {
        return !string.IsNullOrEmpty(_currentClient.Id)
            ? Key(RateLimitPartitionStrategy.Client, _currentClient.Id)
            : IpKey(context);
    }

    // Kullanici ve endpoint bilesimine gore anahtar; oturum yoksa IP+endpoint'e duser.
    private string UserAndEndpointKey(HttpContext context)
    {
        var identity = _currentUser.Id is { } userId ? userId.ToString() : Ip(context);
        return Key(RateLimitPartitionStrategy.UserAndEndpoint, identity, Endpoint(context));
    }

    // Tenant ve kullanici bilesimine gore anahtar; oturum yoksa IP'ye duser.
    private string TenantAndUserKey(HttpContext context)
    {
        if (_currentUser.Id is not { } userId)
        {
            return IpKey(context);
        }

        var tenant = _currentTenant.Id is { } tenantId
            ? tenantId.ToString()
            : RateLimitConsts.UnknownPartitionKey;
        return Key(RateLimitPartitionStrategy.TenantAndUser, tenant, userId.ToString());
    }

    // Istemci IP adresine gore anahtar.
    private static string IpKey(HttpContext context)
    {
        return Key(RateLimitPartitionStrategy.Ip, Ip(context));
    }

    // Ham IP metnini dondurur; bulunamazsa bilinmeyen kimligi kullanir.
    private static string Ip(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? RateLimitConsts.UnknownPartitionKey;
    }

    // Endpoint kimligini dondurur; bulunamazsa istek yolunu kullanir.
    private static string Endpoint(HttpContext context)
    {
        return context.GetEndpoint()?.DisplayName ?? context.Request.Path.ToString();
    }

    // Strateji on ekini ve kimlik parcalarini ayiracla birlestirip anahtar uretir.
    private static string Key(string strategy, params string[] parts)
    {
        return strategy + RateLimitConsts.PartitionKeySeparator +
               string.Join(RateLimitConsts.PartitionKeySeparator, parts);
    }
}
