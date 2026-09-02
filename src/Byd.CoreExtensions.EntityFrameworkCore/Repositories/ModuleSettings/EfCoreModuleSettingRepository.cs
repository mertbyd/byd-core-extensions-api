using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// ModuleSetting extended repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: ModuleSetting icin generic, bulk ve DataTable repository yeteneklerini saglar; ayrica kapsam cozumleme sorgusunu yazar.
// sistemdeki gorevi: IModuleSettingRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
// mirastan gelen:
//   EfExtendedRepository -> FindAsync, GetListAsync, GetQueryableAsync (ayrica metot yazilmaz)
// burada yazilan: FindForScopeAsync
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

    // Tenant satirini ve host satirini tek sorguda alir, tenant satiri varsa onu tercih eder.
    // Iki ayri sorgu yerine siralama kullanilir; boylece host satirina dusme karari veritabaninda verilir.
    public virtual async Task<ModuleSetting?> FindForScopeAsync(
        string moduleName,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(setting => setting.ModuleName == moduleName &&
                              (setting.TenantId == tenantId || setting.TenantId == null))
            .OrderByDescending(setting => setting.TenantId != null)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
