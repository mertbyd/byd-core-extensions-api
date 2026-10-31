using FluentValidation;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.EmailTemplates;
using Byd.CoreExtensions.ExceptionCodes;

namespace Byd.CoreExtensions.Validators.EmailTemplates;

/// <summary>
/// E-posta sablonu create ve update isteklerinin ortak sekil kurallarini tanimlar.
/// </summary>
// islevi: Zorunluluk ve uzunluk hatalarini veritabanina gitmeden reddeder.
// sistemdeki gorevi: Create ve update validator'larinin kararli hata kodu kullanan ortak tabanidir.
public abstract class EmailTemplateInputDtoValidatorBase<TDto> : AbstractValidator<TDto>
    where TDto : EmailTemplateInputDto
{
    protected EmailTemplateInputDtoValidatorBase()
    {
        RuleFor(input => input.Name)
            .NotEmpty().WithErrorCode(EmailTemplateExceptionCodes.NameRequired)
            .MaximumLength(EmailTemplateConsts.MaxNameLength)
            .WithErrorCode(EmailTemplateExceptionCodes.NameTooLong);
        RuleFor(input => input.Subject)
            .NotEmpty().WithErrorCode(EmailTemplateExceptionCodes.SubjectRequired)
            .MaximumLength(EmailTemplateConsts.MaxSubjectLength)
            .WithErrorCode(EmailTemplateExceptionCodes.SubjectTooLong);
        RuleFor(input => input.Body)
            .NotEmpty().WithErrorCode(EmailTemplateExceptionCodes.BodyRequired);
    }
}
