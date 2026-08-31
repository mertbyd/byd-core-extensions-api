using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
namespace Byd.CoreExtensions.Idempotency;
/// <summary>
/// Bir HTTP isteginden kararli idempotency imzasi uretir.
/// </summary>
// islevi: Kiraci, kullanici, metot, yol ve govde ozetini tek SHA256 imzasina indirger.
// sistemdeki gorevi: Filtrenin mukerrer istekleri istemci header'i olmadan ayirt etmesini saglayan is kuralidir.
public class IdempotencyRequestHasher : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public IdempotencyRequestHasher(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }
    // Istegin kimligini kanoniklestirir: kiraci, kullanici, metot, yol ve govde tek ozete indirgenir.
    public virtual string Compute(HttpContext context, byte[] body)
    {
        var bodyHash = Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant();
        var payload = string.Join(
            '\n',
            _currentTenant.Id?.ToString() ?? string.Empty,
            _currentUser.Id?.ToString() ?? string.Empty,
            context.Request.Method,
            context.Request.Path.ToString(),
            bodyHash);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)))
            .ToLowerInvariant();
    }
}
