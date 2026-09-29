using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.CommentQueries.GetComment
{
    internal class GetCommentQueryHandler(IBaseRepository<Comment> repository) : IRequestHandler<GetCommentQuery, Result<CommentResponseDTO>>
    {
        public async Task<Result<CommentResponseDTO>> Handle(GetCommentQuery request, CancellationToken cancellationToken)
        {
            Guid commentId = request.Id;

            var commentResponseDto = await repository.GetByIdProjectedAsync<CommentResponseDTO>(commentId, cancellationToken);
                        
            if (commentResponseDto is null)
            {
                return Result<CommentResponseDTO>.Failure([CommentErrors.CommentNotFound(commentId)]);
            }            
            
            return Result<CommentResponseDTO>.Success(commentResponseDto);
        }
    }
}
