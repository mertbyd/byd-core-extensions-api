using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Byd.CoreExtensions.Dtos.RateLimit;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Services.RateLimit;

/// <summary>
/// Modul ayari tam CRUD kullanim senaryolarini tanimlar.
/// </summary>
// islevi: ModuleSetting okuma, listeleme, olusturma, guncelleme ve silme kontratini acar.
// sistemdeki gorevi: Controller ile Application implementasyonu arasindaki public servis siniridir.
public interface IModuleSettingAppService : IApplicationService
{
    // Kimligi verilen modul ayarini getirir.
    Task<ModuleSettingDto> GetAsync(Guid id);

    // Tum modul ayari kayitlarini getirir.
    Task<List<ModuleSettingDto>> GetListAsync();

    // Dogrulanmis girdiden yeni modul ayari olusturur.
    Task<ModuleSettingDto> CreateAsync(CreateModuleSettingDto input);

    // Kimligi verilen modul ayarinin degistirilebilir alanlarini gunceller.
    Task<ModuleSettingDto> UpdateAsync(Guid id, UpdateModuleSettingDto input);

    // Kimligi verilen modul ayarini siler.
    Task DeleteAsync(Guid id);
}
