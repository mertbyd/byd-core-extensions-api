using FluentValidation;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Dtos.ApiKeys;
using Byd.CoreExtensions.ExceptionCodes;

namespace Byd.CoreExtensions.Validators.ApiKeys;

/// <summary>
/// ApiKey uretme istegini sekil kurallariyla dogrular.
/// </summary>
// islevi: Ad alanindaki zorunluluk ve uzunluk hatalarini veritabanina gitmeden kararli hata koduyla reddeder.
// sistemdeki gorevi: Uretim endpointinin public girdi validator'idir.
public sealed class CreateApiKeyDtoValidator : AbstractValidator<CreateApiKeyDto>
{
    public CreateApiKeyDtoValidator()
    {
        RuleFor(input => input.Name)
            .NotEmpty().WithErrorCode(ApiKeyExceptionCodes.NameRequired)
            .MaximumLength(ApiKeyConsts.MaxNameLength)
            .WithErrorCode(ApiKeyExceptionCodes.NameTooLong);
    }
}
