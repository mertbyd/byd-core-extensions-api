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

namespace Byd.CoreExtensions.ModuleSettings;

/// <summary>
/// Modul ayari okumalarini repository ve dagitik cache uzerinden karsilar.
/// </summary>
// islevi: Beyan edilen modul adinin kiraci veya host kapsamli satirini okur ve merkezi cache modeline donusturur.
// sistemdeki gorevi: Domain.Shared'daki okuma sozlesmesinin tek uygulamasi olarak tablo erisimini tum tuketiciler icin merkezilestirir.
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
    // Host kapsaminda taze ayar okuyup eski host cache kaydini yeni goruntuyle degistirir.
    public async Task<ModuleSettingCacheModel?> RefreshHostAsync(CancellationToken cancellationToken = default)
    {
        using (_currentTenant.Change(null))
        {
            var moduleName = _moduleNameProvider.Value;
            await _settingCache.RemoveAsync(moduleName, token: cancellationToken);
            var setting = ToCacheModel(await _moduleSettingRepository.FindForScopeAsync(
                moduleName,
                tenantId: null,
                cancellationToken));
            if (setting is null)
            {
                return null;
            }

            await _settingCache.SetAsync(
                moduleName,
                setting,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromSeconds(ModuleSettingConsts.DefaultTtlSeconds)
                },
                token: cancellationToken);
            return setting;
        }
    }
    // ModuleSetting entity'sini tum ayar tuketicilerinin kullandigi merkezi cache modeline donusturur.
    private static ModuleSettingCacheModel? ToCacheModel(ModuleSetting? setting)
    {
        return setting is null
            ? null
            : new ModuleSettingCacheModel
            {
                IsEnabled = setting.IsEnabled,
                IdempotencyEnabled = setting.IdempotencyEnabled,
                IdempotencyTtlSeconds = setting.IdempotencyTtlSeconds,
                EventBoxEnabled = setting.EventBoxEnabled,
                FailedEventRetryUnlimited = setting.FailedEventRetryUnlimited,
                FailedEventRetryWindowSeconds = setting.FailedEventRetryWindowSeconds,
                RateLimitEnabled = setting.RateLimitEnabled,
                RateLimitPermitLimit = setting.RateLimitPermitLimit,
                RateLimitWindowSeconds = setting.RateLimitWindowSeconds,
                TtlSeconds = setting.TtlSeconds
            };
    }
}
