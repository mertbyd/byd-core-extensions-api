using Byd.CoreExtensions.Dtos.ModuleSettings;

namespace Byd.CoreExtensions.Validators.ModuleSettings;

/// <summary>
/// ModuleSetting create istegini ortak alan kurallariyla dogrular.
/// </summary>
// islevi: CreateModuleSettingDto icin FluentValidation kurallarini etkinlestirir.
// sistemdeki gorevi: Create endpointinin public girdi validator'idir.
public sealed class CreateModuleSettingDtoValidator
    : ModuleSettingInputDtoValidatorBase<CreateModuleSettingDto>
{
}
