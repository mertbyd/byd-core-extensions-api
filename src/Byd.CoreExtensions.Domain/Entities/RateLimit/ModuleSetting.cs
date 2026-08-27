using System;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.CoreExtensions.Entities.RateLimit;

/// <summary>
/// Bir modulun merkezi calisma ve rate limit ayarlarini tasir.
/// </summary>
// islevi: Modul bazli etkinlik, TTL ve rate limit alanlarini audited aggregate olarak kalici tutar.
// sistemdeki gorevi: Is kurallarini ModuleSettingManager'a birakan host seviyesinde global veri kabugudur.
public class ModuleSetting : AuditedAggregateRoot<Guid>
{
    public string ModuleName { get; internal set; } = default!;
    public bool IsEnabled { get; internal set; }
    public int TtlSeconds { get; internal set; } = ModuleSettingConsts.DefaultTtlSeconds;
    public bool RateLimitEnabled { get; internal set; }
    public int RateLimitPermitLimit { get; internal set; } = ModuleSettingConsts.DefaultRateLimitPermitLimit;
    public int RateLimitWindowSeconds { get; internal set; } = ModuleSettingConsts.DefaultRateLimitWindowSeconds;

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
