using ArticlesService.Domain.DomainEvents;
using ArticlesService.Domain.Entities.Base;
using ArticlesService.Domain.Enums;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;

namespace ArticlesService.Domain.Entities
{
    public class Article : BaseAuditableEntity
    {
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string? Content { get; set; } 
        public string? PreviewText { get; set; }

        public ArticleStatus Status { get; private set; } = ArticleStatus.Draft;

        // Храним относительный путь: "/uploads/articles/cover_123.jpg"
        //public string? CoverImagePath { get; set; }

        public ICollection<ArticleCategory> Categories { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];

        public DateTimeOffset? PublicationDate { get; private set; }

        public Result<bool> Publish(DateTimeOffset publishedAt)
        {
            if (Status == ArticleStatus.Published)
                return Result<bool>.Failure([ArticleErrors.ArticleAlreadyPublished(Id)]);

            // статья должна иметь контент для публикации
            if (string.IsNullOrWhiteSpace(Content))
                return Result<bool>.Failure([ArticleErrors.EmptyContentCannotBePublished(Id)]);

            PublicationDate = publishedAt;
            Status = ArticleStatus.Published;

            RaiseDomainEvent(new ArticlePublishedDomainEvent(Id, Title, AuthorId, PublicationDate.Value));

            return Result<bool>.Success(true);
        }
    }    
}
