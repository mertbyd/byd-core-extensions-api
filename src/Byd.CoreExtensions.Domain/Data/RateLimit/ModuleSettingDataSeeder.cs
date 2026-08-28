using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Models.RateLimit;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions.Data.RateLimit;

/// <summary>
/// Bu servisin ModuleSetting satirini varsayilan degerlerle idempotent olarak olusturur.
/// </summary>
// islevi: CurrentModuleName satiri yoksa Manager ile kurar ve repository uzerinden kalici hale getirir.
// sistemdeki gorevi: Rate limit okuyucusunun yonetilebilir bir baslangic satirina sahip olmasini saglar; mevcut admin degerlerini korur.
public class ModuleSettingDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly ModuleSettingManager _manager;
    private readonly IModuleSettingRepository _repository;

    public ModuleSettingDataSeeder(
        ModuleSettingManager manager,
        IModuleSettingRepository repository)
    {
        _manager = manager;
        _repository = repository;
    }

    // Bu modulun satiri yoksa tablo varsayilanlariyla ekler; mevcut satira dokunmaz.
    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var current = await _repository.FindAsync(
            setting => setting.ModuleName == ModuleSettingConsts.CurrentModuleName,
            cancellationToken: CancellationToken.None);
        if (current is not null)
        {
            return;
        }

        var model = new ModuleSettingCreateModel
        {
            ModuleName = ModuleSettingConsts.CurrentModuleName,
            TtlSeconds = ModuleSettingConsts.DefaultTtlSeconds,
            RateLimitPermitLimit = ModuleSettingConsts.DefaultRateLimitPermitLimit,
            RateLimitWindowSeconds = ModuleSettingConsts.DefaultRateLimitWindowSeconds
        };
        var entity = await _manager.CreateAsync(model, CancellationToken.None);
        await _repository.InsertAsync(
            entity,
            autoSave: true,
            cancellationToken: CancellationToken.None);
    }
}
