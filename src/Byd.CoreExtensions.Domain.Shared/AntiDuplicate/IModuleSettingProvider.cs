using System.Threading;
using System.Threading.Tasks;

namespace Byd.CoreExtensions.AntiDuplicate;

/// <summary>
/// Istegin kapsamina ait modul ayarini koruma tarafina okur.
/// </summary>
// islevi: Modul adini ve kiraci kapsamini kendi icinde cozup ayar satirinin cache'lenmis goruntusunu doner; satir yoksa null doner.
// sistemdeki gorevi: Korumanin ayar tablosuna erisimini tersine cevirir. Middleware repository'yi ve entity'yi hic gormedigi icin Domain'e bagimli olmadan calisir; okuma bu sozlesmeyi karsilayan tarafa aittir.
public interface IModuleSettingProvider
{
    Task<ModuleSettingCacheModel?> FindCurrentAsync(CancellationToken cancellationToken = default);
}
