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
                .Map(dest => dest.CommentsTotalCount, src => src.Comments.Count)
                .Map(dest => dest.AuthorName, src => src.Author.UserName); 

            config.NewConfig<UpdateArticleCommand, Article>()
                .IgnoreNullValues(true)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.Tags) // коллекции обновляются вручную в команде
                .Ignore(dest => dest.Categories)
                .Ignore(dest => dest.Comments)
                .Ignore(dest => dest.PublicationDate);
                

            config.NewConfig<(Article article, string AuthorName), ArticleResponseDTO>()
               .Map(dest => dest.AuthorName, src => src.AuthorName)
               .Map(dest => dest, src => src.article);

            config.NewConfig<Article, ArticleResponseDTO>()
                .Map(dest => dest.AuthorName, src => src.Author.UserName);
        } 
    }
}
