using System;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.CoreExtensions.Entities.ModuleSettings;

/// <summary>
/// Bir modulun merkezi calisma, rate limit, idempotency ve olay kutusu ayarlarini tasir.
/// </summary>
// islevi: Modul bazli etkinlik, TTL, rate limit, idempotency ve olay kutusu alanlarini audited aggregate olarak kalici tutar.
// sistemdeki gorevi: Is kurallarini ModuleSettingManager'a birakan veri kabugudur. Satirin kimligi ModuleName ile TenantId bilesimidir; ayni modulun host satiri ile tenant satiri yan yana yasar.
public class ModuleSetting : AuditedAggregateRoot<Guid>
{
    public string ModuleName { get; internal set; } = default!;

    // Satirin gecerli oldugu kapsam; null ise host (tum tenant'lar icin gecerli) satiridir.
    // Bilerek IMultiTenant degildir: ABP'nin global filtresi izolasyon uygular ve tenant istegi
    // sirasinda host satirini gizler; bu tablonun sozlesmesi ise izolasyon degil, tenant satiri
    // yoksa host satirina dusen bir cozumleme zinciridir.
    public Guid? TenantId { get; internal set; }
    public bool IsEnabled { get; internal set; }
    public int TtlSeconds { get; internal set; } = ModuleSettingConsts.DefaultTtlSeconds;
    public bool RateLimitEnabled { get; internal set; }
    public int RateLimitPermitLimit { get; internal set; } = ModuleSettingConsts.DefaultRateLimitPermitLimit;
    public int RateLimitWindowSeconds { get; internal set; } = ModuleSettingConsts.DefaultRateLimitWindowSeconds;
    public bool IdempotencyEnabled { get; internal set; }
    public int IdempotencyTtlSeconds { get; internal set; } = IdempotencyConsts.DefaultTtlSeconds;
    public bool EventBoxEnabled { get; internal set; }
    public bool FailedEventRetryUnlimited { get; internal set; }
    public int FailedEventRetryWindowSeconds { get; internal set; }
        = EventBoxConsts.DefaultFailedEventRetryWindowSeconds;

    // EF Core materializasyonu icin parametresiz ctor.
    protected ModuleSetting()
    {
    }

    // Manager tarafindan uretilen kimlikle bos veri kabugunu kurar.
    internal ModuleSetting(Guid id)
        : base(id)
    {
    }
}
