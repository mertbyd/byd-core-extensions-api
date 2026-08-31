using Byd.CoreExtensions.Dtos.ModuleSettings;

namespace Byd.CoreExtensions.Validators.ModuleSettings;

/// <summary>
/// ModuleSetting update istegini ortak alan kurallariyla dogrular.
/// </summary>
// islevi: UpdateModuleSettingDto icin FluentValidation kurallarini etkinlestirir.
// sistemdeki gorevi: Update endpointinin public girdi validator'idir.
public sealed class UpdateModuleSettingDtoValidator
    : ModuleSettingInputDtoValidatorBase<UpdateModuleSettingDto>
{
}
