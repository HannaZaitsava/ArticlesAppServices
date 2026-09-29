using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.TagQueries.GetAllTags
{    
    internal class GetAllTagsQueryHandler(
        ITagRepository tagRepository)
        : IRequestHandler<GetAllTagsQuery, Result<OffsetPagedResult<TagShortInfoResponseDTO>>>
    {
        public async Task<Result<OffsetPagedResult<TagShortInfoResponseDTO>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
        {
            var tags = await tagRepository.GetOffsetPagedListProjectedAsync<TagShortInfoResponseDTO>(request.PaginationParameters, cancellationToken);

            return Result<OffsetPagedResult<TagShortInfoResponseDTO>>.Success(tags);
        }
    }
}
