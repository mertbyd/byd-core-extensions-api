using Byd.CoreExtensions.Dtos.RateLimit;

namespace Byd.CoreExtensions.Validators.RateLimit;

/// <summary>
/// ModuleSetting create istegini ortak alan kurallariyla dogrular.
/// </summary>
// islevi: CreateModuleSettingDto icin FluentValidation kurallarini etkinlestirir.
// sistemdeki gorevi: Create endpointinin public girdi validator'idir.
public sealed class CreateModuleSettingDtoValidator
    : ModuleSettingInputDtoValidatorBase<CreateModuleSettingDto>
{
}
