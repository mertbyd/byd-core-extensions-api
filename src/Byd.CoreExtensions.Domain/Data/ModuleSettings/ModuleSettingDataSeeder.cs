using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Models.ModuleSettings;
using Byd.CoreExtensions.Providers;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions.Data.ModuleSettings;

/// <summary>
/// Bu servisin ModuleSetting satirini varsayilan degerlerle idempotent olarak olusturur.
/// </summary>
// islevi: Host'un beyan ettigi modul adina ait satir yoksa Manager ile kurar ve repository uzerinden kalici hale getirir.
// sistemdeki gorevi: Rate limit okuyucusunun yonetilebilir bir baslangic satirina sahip olmasini saglar; mevcut admin degerlerini korur.
public class ModuleSettingDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly ModuleSettingManager _manager;
    private readonly CurrentModuleNameProvider _moduleNameProvider;
    private readonly IModuleSettingRepository _repository;

    public ModuleSettingDataSeeder(
        ModuleSettingManager manager,
        CurrentModuleNameProvider moduleNameProvider,
        IModuleSettingRepository repository)
    {
        _manager = manager;
        _moduleNameProvider = moduleNameProvider;
        _repository = repository;
    }

    // Bu modulun satiri yoksa tablo varsayilanlariyla ekler; mevcut satira dokunmaz.
    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var current = await _repository.FindAsync(
            setting => setting.ModuleName == _moduleNameProvider.Value,
            cancellationToken: CancellationToken.None);
        if (current is not null)
        {
            return;
        }

        var model = new ModuleSettingCreateModel
        {
            ModuleName = _moduleNameProvider.Value,
            TtlSeconds = ModuleSettingConsts.DefaultTtlSeconds,
            RateLimitPermitLimit = ModuleSettingConsts.DefaultRateLimitPermitLimit,
            RateLimitWindowSeconds = ModuleSettingConsts.DefaultRateLimitWindowSeconds,
            IdempotencyTtlSeconds = IdempotencyConsts.DefaultTtlSeconds
        };
        var entity = await _manager.CreateAsync(model, CancellationToken.None);
        await _repository.InsertAsync(
            entity,
            autoSave: true,
            cancellationToken: CancellationToken.None);
    }
}
