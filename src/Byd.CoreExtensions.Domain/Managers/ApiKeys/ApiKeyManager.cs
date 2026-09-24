using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.ApiKeys;

namespace Byd.CoreExtensions.Managers;

/// <summary>
/// API anahtari uretme, normalizasyon ve varlik kurallarini yonetir.
/// </summary>
// islevi: Ad alanini kanoniklestirir, kriptografik olarak guvenli anahtar uretir ve data-shell entity mutasyonunu yapar.
// sistemdeki gorevi: AppService'i is kararindan ve entity'yi davranistan uzak tutan domain sahibidir; anahtar degeri disaridan hic alinmaz.
public class ApiKeyManager : BaseManager<ApiKey, Guid>
{
    // ApiKey extended repository'sini ortak guard tabanina devreder.
    public ApiKeyManager(IApiKeyRepository repository)
        : base(repository)
    {
    }

    // Kimligi verilen anahtari getirir; yoksa BaseManager lokalize NotFound firlatir.
    public virtual async Task<ApiKey> EnsureExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return (await TryGetByIdAsync(id, throwIfNotExists: true, cancellationToken))!;
    }

    // Create modelini normalize edip yeni anahtar degeriyle ApiKey entity'sine donusturur.
    public virtual ApiKey Create(ApiKeyCreateModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new ApiKey(GuidGenerator.Create(), GenerateKey())
        {
            Name = NormalizeRequiredText(model.Name),
            ExpiresAt = model.ExpiresAt,
            TenantId = model.TenantId,
            OrganizationId = model.OrganizationId
        };
    }

    // Kriptografik olarak guvenli rastgele bayttan URL guvenli anahtar uretir.
    private static string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(ApiKeyConsts.GeneratedKeyByteLength);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
