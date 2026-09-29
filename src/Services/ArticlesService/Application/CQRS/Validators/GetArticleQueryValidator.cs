using ArticlesService.Application.CQRS.Queries.ArticleQueries.GetArticleQuery;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public sealed class GetArticleQueryValidator : AbstractValidator<GetArticleQuery>
    {
        public GetArticleQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Article Id is required");
        }
    }
}
