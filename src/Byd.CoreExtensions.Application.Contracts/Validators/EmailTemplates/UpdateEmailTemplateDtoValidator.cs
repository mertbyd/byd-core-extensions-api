using Byd.CoreExtensions.Dtos.EmailTemplates;

namespace Byd.CoreExtensions.Validators.EmailTemplates;

/// <summary>
/// EmailTemplate update istegini ortak alan kurallariyla dogrular.
/// </summary>
// islevi: UpdateEmailTemplateDto icin FluentValidation kurallarini etkinlestirir.
// sistemdeki gorevi: Update endpointinin public girdi validator'idir.
public sealed class UpdateEmailTemplateDtoValidator
    : EmailTemplateInputDtoValidatorBase<UpdateEmailTemplateDto>
{
}
