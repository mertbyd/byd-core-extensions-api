using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Providers;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
namespace Byd.CoreExtensions.AntiDuplicate;
/// <summary>
/// Mukerrer istek korumasinin ayar okumasini repository uzerinden karsilar.
/// </summary>
// islevi: Beyan edilen modul adinin istek kapsamindaki satirini TTL boyunca cache'ten okur, cache bosken tabloya bir kez gider ve sade goruntuye cevirir.
// sistemdeki gorevi: Domain.Shared'daki okuma sozlesmesinin tek uygulamasi. Repository ve entity bu katmanda kaldigi icin koruma tarafi Domain'e bagimli olmadan calisir.
public class ModuleSettingProvider : IModuleSettingProvider, ITransientDependency
{
    private readonly IModuleSettingRepository _moduleSettingRepository;
    private readonly CurrentModuleNameProvider _moduleNameProvider;
    private readonly IDistributedCache<ModuleSettingCacheModel> _settingCache;
    private readonly ICurrentTenant _currentTenant;
    public ModuleSettingProvider(
        IModuleSettingRepository moduleSettingRepository,
        CurrentModuleNameProvider moduleNameProvider,
        IDistributedCache<ModuleSettingCacheModel> settingCache,
        ICurrentTenant currentTenant)
    {
        _moduleSettingRepository = moduleSettingRepository;
        _moduleNameProvider = moduleNameProvider;
        _settingCache = settingCache;
        _currentTenant = currentTenant;
    }
    // Ayar satirini TTL suresince cache'ten okur; sure dolunca tabloyu bir kez yeniden okur.
    // Istek bir tenant baglamindaysa once o tenant'in satiri, yoksa host satiri cozulur. Cache anahtarina
    // tenant eklenmez: ABP'nin cache anahtari normalizer'i mevcut tenant'i anahtara kendisi tasir.
    public async Task<ModuleSettingCacheModel?> FindCurrentAsync(CancellationToken cancellationToken = default)
    {
        var moduleName = _moduleNameProvider.Value;
        return await _settingCache.GetOrAddAsync(
            moduleName,
            async () => ToCacheModel(await _moduleSettingRepository.FindForScopeAsync(
                moduleName,
                _currentTenant.Id,
                cancellationToken))!,
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromSeconds(ModuleSettingConsts.DefaultTtlSeconds)
            },
            token: cancellationToken);
    }
    // ModuleSetting entity'sini korumaya gereken sade cache modeline donusturur.
    private static ModuleSettingCacheModel? ToCacheModel(ModuleSetting? setting)
    {
        return setting is null
            ? null
            : new ModuleSettingCacheModel
            {
                IsEnabled = setting.IsEnabled,
                IdempotencyEnabled = setting.IdempotencyEnabled,
                IdempotencyTtlSeconds = setting.IdempotencyTtlSeconds
            };
    }
}
