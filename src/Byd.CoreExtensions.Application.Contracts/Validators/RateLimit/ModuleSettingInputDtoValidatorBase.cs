using FluentValidation;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.RateLimit;
using Byd.CoreExtensions.ExceptionCodes;

namespace Byd.CoreExtensions.Validators.RateLimit;

/// <summary>
/// Modul ayari create ve update isteklerinin ortak sekil kurallarini tanimlar.
/// </summary>
// islevi: Zorunluluk, uzunluk ve sayisal aralik hatalarini veritabanina gitmeden reddeder.
// sistemdeki gorevi: Create ve update validator'larinin kararli hata kodu kullanan ortak tabanidir.
public abstract class ModuleSettingInputDtoValidatorBase<TDto> : AbstractValidator<TDto>
    where TDto : ModuleSettingInputDto
{
    protected ModuleSettingInputDtoValidatorBase()
    {
        RuleFor(input => input.ModuleName)
            .NotEmpty().WithErrorCode(ModuleSettingExceptionCodes.ModuleNameRequired)
            .MaximumLength(ModuleSettingConsts.MaxModuleNameLength)
            .WithErrorCode(ModuleSettingExceptionCodes.ModuleNameTooLong);
        RuleFor(input => input.TtlSeconds)
            .InclusiveBetween(ModuleSettingConsts.MinTtlSeconds, ModuleSettingConsts.MaxTtlSeconds)
            .WithErrorCode(ModuleSettingExceptionCodes.InvalidTtlSeconds);
        RuleFor(input => input.RateLimitPermitLimit)
            .InclusiveBetween(ModuleSettingConsts.MinPermitLimit, ModuleSettingConsts.MaxPermitLimit)
            .WithErrorCode(ModuleSettingExceptionCodes.InvalidRateLimitPermitLimit);
        RuleFor(input => input.RateLimitWindowSeconds)
            .InclusiveBetween(ModuleSettingConsts.MinWindowSeconds, ModuleSettingConsts.MaxWindowSeconds)
            .WithErrorCode(ModuleSettingExceptionCodes.InvalidRateLimitWindowSeconds);
    }
}
