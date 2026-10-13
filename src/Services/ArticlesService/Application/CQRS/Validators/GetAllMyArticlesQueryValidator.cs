using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.CQRS.Queries.My.GetAllMyArticlesQuery;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{    
    public class GetAllMyArticlesQueryValidator : AbstractValidator<GetAllMyArticlesQuery>
    {
        public GetAllMyArticlesQueryValidator()
        {
            RuleFor(x => x.PaginationParameters)
                .NotEmpty()
                // Передаем фабрику, которая создает и настраивает валидатор в рантайме
                .SetValidator(_ => new OffsetPaginationParametersValidator()
                    .Configure(PaginationConstants.ArticlesDefaultPageSize));

            //RuleFor(x => x.Sorts)
            //     .IsValidSortItemForEntity<GetAllMyArticlesQuery, ArticleSortItem, ArticleSortField, Article>();           
        }
    }
}
