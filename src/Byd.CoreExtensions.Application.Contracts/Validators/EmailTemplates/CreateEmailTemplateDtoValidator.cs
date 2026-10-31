using FluentValidation;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.EmailTemplates;
using Byd.CoreExtensions.ExceptionCodes;

namespace Byd.CoreExtensions.Validators.EmailTemplates;

/// <summary>
/// EmailTemplate create istegini ortak alan kurallari ve olay, dil kurallariyla dogrular.
/// </summary>
// islevi: CreateEmailTemplateDto icin FluentValidation kurallarini etkinlestirir.
// sistemdeki gorevi: Create endpointinin public girdi validator'idir; olay ve dil yalniz olusturmada alindigi icin
// kurallari bu sinifta durur.
public sealed class CreateEmailTemplateDtoValidator
    : EmailTemplateInputDtoValidatorBase<CreateEmailTemplateDto>
{
    public CreateEmailTemplateDtoValidator()
    {
        RuleFor(input => input.EmailEventId)
            .NotEmpty().WithErrorCode(EmailTemplateExceptionCodes.EmailEventRequired);
        RuleFor(input => input.CultureName)
            .NotEmpty().WithErrorCode(EmailTemplateExceptionCodes.CultureNameRequired)
            .MaximumLength(EmailTemplateConsts.MaxCultureNameLength)
            .WithErrorCode(EmailTemplateExceptionCodes.CultureNameTooLong);
    }
}
