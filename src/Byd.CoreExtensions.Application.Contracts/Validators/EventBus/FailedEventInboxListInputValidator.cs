using FluentValidation;
using Byd.CoreExtensions.Dtos.EventBus;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Querying;

namespace Byd.CoreExtensions.Validators.EventBus;

/// <summary>
/// Basarisiz olay kutusu liste isteginin sayfalama sinirlarini dogrular.
/// </summary>
// islevi: Sayfa numarasi ile sayfa boyutunu desteklenen DataTable araliginda tutar.
// sistemdeki gorevi: Dinamik sorgu repository'sine yalnizca sinirlari belli liste isteklerinin girmesini saglar.
public class FailedEventInboxListInputValidator : AbstractValidator<FailedEventInboxListInput>
{
    // Basarisiz olay liste isteginin sayfalama kurallarini tanimlar.
    public FailedEventInboxListInputValidator()
    {
        RuleFor(input => input.Page)
            .GreaterThanOrEqualTo(DataTablePaging.FirstPage)
            .WithErrorCode(GeneralExceptionCodes.InvalidPage);
        RuleFor(input => input.PageSize)
            .InclusiveBetween(1, DataTablePaging.MaxPageSize)
            .WithErrorCode(GeneralExceptionCodes.InvalidPageSize);
    }
}
