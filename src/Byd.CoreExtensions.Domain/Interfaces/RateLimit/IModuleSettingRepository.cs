using Byd.CoreExtensions.Entities.RateLimit;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Modul ayari aggregate'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: ModuleSetting CRUD ve sorgu yeteneklerini Domain katmanina soyutlar.
// sistemdeki gorevi: Manager/AppService ile EF Core repository uygulamasi arasindaki veri erisim siniridir.
public interface IModuleSettingRepository : IExtendedRepository<ModuleSetting>
{
}
