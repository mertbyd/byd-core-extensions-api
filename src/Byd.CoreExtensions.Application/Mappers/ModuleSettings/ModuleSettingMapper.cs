using System.Collections.Generic;
using Byd.CoreExtensions.Dtos.ModuleSettings;
using Byd.CoreExtensions.Entities.ModuleSettings;
using Byd.CoreExtensions.Models.ModuleSettings;
using Riok.Mapperly.Abstractions;

namespace Byd.CoreExtensions.Mappers.ModuleSettings;

/// <summary>
/// ModuleSetting DTO, domain model ve entity eslemelerini tanimlar.
/// </summary>
// islevi: CRUD girdileri ile ciktilari Mapperly tarafindan uretilen kodla donusturur.
// sistemdeki gorevi: ModuleSetting katmanlari arasindaki alan tasimanin tek sahibidir.
[Mapper]
public partial class ModuleSettingMapper
{
    public partial ModuleSettingCreateModel Map(CreateModuleSettingDto source);
    public partial ModuleSettingUpdateModel Map(UpdateModuleSettingDto source);
    public partial ModuleSettingDto Map(ModuleSetting source);
    public partial ModuleSettingRateLimitDto MapRateLimit(ModuleSetting source);
    public partial ModuleSettingIdempotencyDto MapIdempotency(ModuleSetting source);
    public partial ModuleSettingEventBoxDto MapEventBox(ModuleSetting source);
    public partial List<ModuleSettingDto> Map(List<ModuleSetting> source);
}
