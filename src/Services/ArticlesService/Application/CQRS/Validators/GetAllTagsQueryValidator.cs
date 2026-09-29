using ArticlesService.Application.CQRS.Validators;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.CQRS.Queries.TagQueries.GetAllTags;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{    
    public class GetAllTagsQueryValidator : AbstractValidator<GetAllTagsQuery>
    {
        public GetAllTagsQueryValidator()
        {
            RuleFor(x => x.PaginationParameters)
                .NotEmpty()
                // Передаем фабрику, которая создает и настраивает валидатор в рантайме
                .SetValidator(_ => new OffsetPaginationParametersValidator()
                    .Configure(PaginationConstants.TagsDefaultPageSize));
        }
    }
}
