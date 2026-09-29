using ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetArticleCategory;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public sealed class GetArticleCategoryQueryValidator : AbstractValidator<GetArticleCategoryQuery>
    {
        public GetArticleCategoryQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Article category Id is required");
        }
    }
}
