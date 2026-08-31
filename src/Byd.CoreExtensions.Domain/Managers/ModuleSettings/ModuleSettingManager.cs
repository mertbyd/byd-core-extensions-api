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
// islevi: ModuleName alanini kanoniklestirir, tekilligi dogrular ve data-shell entity mutasyonunu yapar.
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
        CancellationToken cancellationToken)
    {
        return (await TryGetByIdAsync(id, throwIfNotExists: true, cancellationToken))!;
    }

    // Create modelini normalize edip benzersiz bir ModuleSetting entity'sine donusturur.
    public virtual async Task<ModuleSetting> CreateAsync(
        ModuleSettingCreateModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        var moduleName = NormalizeRequiredText(model.ModuleName);
        await CheckAlreadyExistsAsync(
            entity => entity.ModuleName == moduleName,
            cancellationToken);
        var entity = new ModuleSetting(GuidGenerator.Create());
        Apply(entity, model, moduleName);
        return entity;
    }

    // Update modelini normalize edip kaydin kendisi haric ModuleName tekilligini dogrular.
    public virtual async Task<ModuleSetting> UpdateAsync(
        ModuleSetting entity,
        ModuleSettingUpdateModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        var moduleName = NormalizeRequiredText(model.ModuleName);
        await CheckAlreadyExistsAsync(
            candidate => candidate.ModuleName == moduleName,
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
        entity.IsEnabled = model.IsEnabled;
        entity.TtlSeconds = model.TtlSeconds;
        entity.RateLimitEnabled = model.RateLimitEnabled;
        entity.RateLimitPermitLimit = model.RateLimitPermitLimit;
        entity.RateLimitWindowSeconds = model.RateLimitWindowSeconds;
    }
}
