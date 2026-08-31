using Byd.CoreExtensions.Entities.ModuleSettings;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// ModuleSetting extended repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: ModuleSetting icin generic, bulk ve DataTable repository yeteneklerini saglar.
// sistemdeki gorevi: IModuleSettingRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
[ExposeServices(typeof(IModuleSettingRepository))]
public class EfCoreModuleSettingRepository
    : EfExtendedRepository<CoreExtensionsDbContext, ModuleSetting, System.Guid>,
        IModuleSettingRepository
{
    // DbContext provider'ini extended repository tabanina devreder.
    public EfCoreModuleSettingRepository(
        IDbContextProvider<CoreExtensionsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
