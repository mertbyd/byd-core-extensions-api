using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.ModuleSettings;

namespace Byd.CoreExtensions.Managers;

/// <summary>
/// Modul ayari olusturma, guncelleme, normalizasyon ve benzersizlik kurallarini yonetir.
/// </summary>
// islevi: ModuleName alanini kanoniklestirir, kapsam basina tekilligi dogrular ve data-shell entity mutasyonunu yapar.
// sistemdeki gorevi: AppService'i is kararindan ve entity'yi davranistan uzak tutan domain sahibidir.
public class ModuleSettingManager : BaseManager<ModuleSetting, Guid>
{
    // ModuleSetting extended repository'sini ortak guard tabanina devreder.
    public ModuleSettingManager(IModuleSettingRepository repository)
        : base(repository)
    {
    }

    // Kimligi verilen modul ayarini getirir; yoksa BaseManager lokalize NotFound firlatir.
    public virtual async Task<ModuleSetting> EnsureExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return (await TryGetByIdAsync(id, throwIfNotExists: true, cancellationToken))!;
    }

    // Create modelini normalize edip benzersiz bir ModuleSetting entity'sine donusturur.
    public virtual async Task<ModuleSetting> CreateAsync(
        ModuleSettingCreateModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var moduleName = NormalizeRequiredText(model.ModuleName);
        // Tekillik kapsam basinadir: ayni modul adi host satiri ve her tenant icin birer kez bulunabilir.
        var tenantId = model.TenantId;
        await CheckAlreadyExistsAsync(
            entity => entity.ModuleName == moduleName && entity.TenantId == tenantId,
            cancellationToken);
        var entity = new ModuleSetting(GuidGenerator.Create());
        Apply(entity, model, moduleName);
        return entity;
    }

    // Update modelini normalize edip kaydin kendisi haric ModuleName tekilligini dogrular.
    public virtual async Task<ModuleSetting> UpdateAsync(
        ModuleSetting entity,
        ModuleSettingUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        var moduleName = NormalizeRequiredText(model.ModuleName);
        var tenantId = model.TenantId;
        await CheckAlreadyExistsAsync(
            candidate => candidate.ModuleName == moduleName && candidate.TenantId == tenantId,
            entity.Id,
            cancellationToken);
        Apply(entity, model, moduleName);
        return entity;
    }

    // Dogrulanmis input alanlarini ModuleSetting veri kabuguna uygular.
    private static void Apply(
        ModuleSetting entity,
        ModuleSettingInputModel model,
        string moduleName)
    {
        entity.ModuleName = moduleName;
        entity.TenantId = model.TenantId;
        entity.IsEnabled = model.IsEnabled;
        entity.TtlSeconds = model.TtlSeconds;
        entity.RateLimitEnabled = model.RateLimitEnabled;
        entity.RateLimitPermitLimit = model.RateLimitPermitLimit;
        entity.RateLimitWindowSeconds = model.RateLimitWindowSeconds;
        entity.IdempotencyEnabled = model.IdempotencyEnabled;
        entity.IdempotencyTtlSeconds = model.IdempotencyTtlSeconds;
        entity.EventBoxEnabled = model.EventBoxEnabled;
        entity.FailedEventRetryUnlimited = model.FailedEventRetryUnlimited;
        entity.FailedEventRetryWindowSeconds = model.FailedEventRetryWindowSeconds;
    }
}
