using System.Threading;
using System.Threading.Tasks;

namespace Byd.CoreExtensions.ModuleSettings;

/// <summary>
/// Modul ayarini istek ve host kapsamlarinda okur.
/// </summary>
// islevi: Modul adini ve uygun kiraci kapsamini kendi icinde cozup ayar satirinin cache goruntusunu doner; satir yoksa null doner.
// sistemdeki gorevi: Ayar tuketicilerinin repository ve entity bagimliligi olmadan tek okuma servisini kullanmasini saglar.
public interface IModuleSettingProvider
{
    // Istek yolunda mevcut kiraci kapsamindan cache'li ayar okur.
    Task<ModuleSettingCacheModel?> FindCurrentAsync(CancellationToken cancellationToken = default);

    // Acilis ve isci yolunda host kapsamindan taze ayar okur ve cache'i yeniler.
    Task<ModuleSettingCacheModel?> RefreshHostAsync(CancellationToken cancellationToken = default);
}
