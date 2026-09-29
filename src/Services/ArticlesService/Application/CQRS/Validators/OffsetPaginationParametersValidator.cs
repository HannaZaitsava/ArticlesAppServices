using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public class OffsetPaginationParametersValidator : AbstractValidator<OffsetPaginationParameters>
    {
        // Конструктор по умолчанию пустой — DI-контейнер его легко зарегистрирует и не упадет
        public OffsetPaginationParametersValidator() { }

        // Метод для ленивой настройки правил
        public OffsetPaginationParametersValidator Configure(int maxPageSize)
        {
            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(PaginationConstants.MinPageIndex)
                    .WithMessage("Page index must be greater than or equal to {ComparisonValue}");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(PaginationConstants.MinPageSize, maxPageSize == 0 ? PaginationConstants.MaxPageSize : maxPageSize)
                    .WithMessage("Page size must be between {From} and {To}");

            return this; // Возвращаем self для fluent-синтаксиса
        }
    }
}
