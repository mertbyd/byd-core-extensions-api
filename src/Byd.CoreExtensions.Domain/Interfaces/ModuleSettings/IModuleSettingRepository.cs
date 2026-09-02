using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities.ModuleSettings;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Modul ayari aggregate'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: ModuleSetting CRUD ve sorgu yeteneklerini Domain katmanina soyutlar.
// sistemdeki gorevi: Manager/AppService ile EF Core repository uygulamasi arasindaki veri erisim siniridir.
public interface IModuleSettingRepository : IExtendedRepository<ModuleSetting>
{
    // Modulun verilen kapsamdaki satirini getirir; tenant satiri yoksa host satirina duser, o da yoksa null doner.
    Task<ModuleSetting?> FindForScopeAsync(
        string moduleName,
        Guid? tenantId,
        CancellationToken cancellationToken = default);
}
