using Byd.CoreExtensions.Dtos.RateLimit;

namespace Byd.CoreExtensions.Validators.RateLimit;

/// <summary>
/// ModuleSetting update istegini ortak alan kurallariyla dogrular.
/// </summary>
// islevi: UpdateModuleSettingDto icin FluentValidation kurallarini etkinlestirir.
// sistemdeki gorevi: Update endpointinin public girdi validator'idir.
public sealed class UpdateModuleSettingDtoValidator
    : ModuleSettingInputDtoValidatorBase<UpdateModuleSettingDto>
{
}
