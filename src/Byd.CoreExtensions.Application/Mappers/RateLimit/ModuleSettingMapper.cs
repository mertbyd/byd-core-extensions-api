using System.Collections.Generic;
using Byd.CoreExtensions.Dtos.RateLimit;
using Byd.CoreExtensions.Entities.RateLimit;
using Byd.CoreExtensions.Models.RateLimit;
using Riok.Mapperly.Abstractions;

namespace Byd.CoreExtensions.Mappers.RateLimit;

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
    public partial List<ModuleSettingDto> Map(List<ModuleSetting> source);
}
