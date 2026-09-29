using ArticlesService.Application.CQRS.Commands.ArticleCommands.UpdateArticle;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Domain.Entities;
using Mapster;

namespace ArticlesService.Application.MappingProfiles
{
    public class ArticleMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {            
            config.NewConfig<Article, ArticleShortInfoResponseDTO>()
                .Map(dest => dest.CommentsTotalCount, src => src.Comments.Count);

            config.NewConfig<UpdateArticleCommand, Article>()
                .IgnoreNullValues(true)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.Tags) // коллекции обновляются вручную в команде
                .Ignore(dest => dest.Categories)
                .Ignore(dest => dest.Comments)
                .Ignore(dest => dest.PublicationDate);
                

            config.NewConfig<(Article article, string creatorName), ArticleResponseDTO>()
               .Map(dest => dest.CreatorName, src => src.creatorName)
               .Map(dest => dest, src => src.article);
        } 
    }
}
