using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.RequestFeatures.CursorPagination;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public class CursorPaginationParametersValidator : AbstractValidator<CursorPaginationParameters>
    {
        public CursorPaginationParametersValidator()
        {
            RuleFor(x => x.Cursor)
                .Must(cursor => cursor == null || !string.IsNullOrWhiteSpace(cursor))
                .WithMessage("Cursor must be null or a non-empty string.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(PaginationConstants.MinPageSize)
                .WithMessage($"PageSize must be at least {PaginationConstants.MinPageSize}.")
                .LessThanOrEqualTo(PaginationConstants.MaxPageSize)
                .WithMessage($"PageSize must not exceed {PaginationConstants.MaxPageSize}.");

            RuleFor(x => x.Direction)
                .IsInEnum()
                .WithMessage("Direction must be a valid PaginationDirection value.");
        }
    }
}
