using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsOffsetPagedQuery;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{    
    public class GetCommentsOffsetPagedQueryValidator : AbstractValidator<GetCommentsOffsetPagedQuery>
    {
        public GetCommentsOffsetPagedQueryValidator()
        {
            RuleFor(x => x.PaginationParameters)
                .NotEmpty()
                // Передаем фабрику, которая создает и настраивает валидатор в рантайме
                .SetValidator(_ => new OffsetPaginationParametersValidator()
                    .Configure(PaginationConstants.CommentsDefaultPageSize));
        }
    }
}
