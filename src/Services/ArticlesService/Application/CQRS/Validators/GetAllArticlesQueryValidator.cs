using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.CQRS.Queries.ArticleQueries.GetAllArticlesQuery;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{    
    public class GetAllArticlesQueryValidator : AbstractValidator<GetAllArticlesQuery>
    {
        public GetAllArticlesQueryValidator()
        {
            RuleFor(x => x.PaginationParameters)
                .NotEmpty()
                // Передаем фабрику, которая создает и настраивает валидатор в рантайме
                .SetValidator(_ => new OffsetPaginationParametersValidator()
                    .Configure(PaginationConstants.ArticlesDefaultPageSize));

           //RuleFor(x => x.Sorts)
           //     .IsValidSortItemForEntity<GetAllArticlesQuery, ArticleSortItem, ArticleSortField, Article>();           
        }
    }
}
