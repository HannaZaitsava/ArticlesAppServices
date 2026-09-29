using ArticlesService.Application.CQRS.Validators;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetAllArticleCategories;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{   
    public class GetAllArticleCategoriesQueryValidator : AbstractValidator<GetAllArticleCategoriesQuery>
    {
        public GetAllArticleCategoriesQueryValidator()
        {
            RuleFor(x => x.PaginationParameters)
                .NotEmpty()
                // Передаем фабрику, которая создает и настраивает валидатор «на лету»
                .SetValidator(_ => new OffsetPaginationParametersValidator()
                    .Configure(PaginationConstants.ArticleCategoriesDefaultPageSize));
        }
    }
}
