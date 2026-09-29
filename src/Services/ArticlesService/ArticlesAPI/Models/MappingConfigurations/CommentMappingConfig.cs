using ArticlesService.Application.CQRS.Commands.CommentCommands.CreateComment;
using ArticlesService.Application.CQRS.Commands.CommentCommands.UpdateComment;
using ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsCursorPagedQuery;
using ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsOffsetPagedQuery;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.ArticlesAPI.Models.Requests;
using ArticlesService.ArticlesAPI.Models.Responses;
using ArticlesService.Domain.Entities;
using Mapster;

namespace ArticlesService.ArticlesAPI.Models.MappingConfigurations
{
    public class CommentMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(Guid id, CreateCommentApiRequest requestBody), CreateCommentCommand>()
               .Map(dest => dest.ArticleId, src => src.id)
               .Map(dest => dest, src => src.requestBody);

            config.NewConfig<(Guid id, UpdateCommentApiRequest requestBody), UpdateCommentCommand>()
               .Map(dest => dest.Id, src => src.id)
               .Map(dest => dest, src => src.requestBody);         

            config.NewConfig<Comment,(Guid Id, string Text)>()
              .Map(dest => dest.Id, src => src.Id)
              .Map(dest => dest.Text, src => src.Text);            

            //config.NewConfig<Comment, CommentResponseDTO>()            
            //    .Ignore(dest => dest.Replies); // Игнорируем рекурсивное свойство для проекций в БД, т.к. сборка дерева ответов будет собрана в отдельно алгоритме

            config.NewConfig<CommentResponseDTO, CommentApiResponse>()
                .Map(d => d.Text, s => s.IsDeleted ? "This comment has been deleted" : s.Text);
                        
            config.NewConfig<(Guid id, GetArticleCommentsOffsetPaginatedApiRequest requestBody), GetCommentsOffsetPagedQuery>()
              .Map(dest => dest.ArticleId, src => src.id)
              .Map(dest => dest.PaginationParameters.PageIndex, src => src.requestBody.PageIndex)
              .Map(dest => dest.PaginationParameters.PageSize, src => src.requestBody.PageSize); 
            
            //config.NewConfig<(Guid id, GetArticleCommentsCursorPaginatedApiRequest requestBody), GetCommentsCursorQuery>()
            //  .Map(dest => dest.ArticleId, src => src.id)
            //  .Map(dest => dest, src => src.requestBody);

            config.NewConfig<(Guid id, GetArticleCommentsCursorPaginatedApiRequest requestBody), GetCommentsCursorQuery>()
                .Map(dest => dest.ArticleId, src => src.id)
                .Map(dest => dest.PaginationParameters.Cursor, src => src.requestBody.Cursor)
                .Map(dest => dest.PaginationParameters.PageSize, src => src.requestBody.PageSize)
                .Map(dest => dest.PaginationParameters.Direction, src => src.requestBody.Direction);
        }
    }
}
