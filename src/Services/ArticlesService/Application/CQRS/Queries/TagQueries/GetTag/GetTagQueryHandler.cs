using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.TagQueries.GetTag
{
    internal class GetTagByIdQueryHandler(IBaseRepository<Tag> repository) : IRequestHandler<GetTagQuery, Result<TagResponseDTO>>
    {
        public async Task<Result<TagResponseDTO>> Handle(GetTagQuery request, CancellationToken cancellationToken)
        {
            Guid tagId = request.Id;

            var tag = await repository.GetByIdProjectedAsync<TagResponseDTO>(tagId, cancellationToken);

            if (tag is null)
            {
                return Result<TagResponseDTO>.Failure([TagErrors.TagNotFound(tagId)]);
            }

            return Result<TagResponseDTO>.Success(tag); 
        }
    }
}
