using ArticlesService.Domain.DomainEvents;
using ArticlesService.Domain.Entities.Base;
using ArticlesService.Domain.Enums;
using ArticlesService.Domain.Errors;

namespace ArticlesService.Domain.Entities
{
    public class Article : BaseAuditableEntity
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;

        public ArticleStatus Status { get; private set; } = ArticleStatus.Draft;

        // Храним относительный путь: "/uploads/articles/cover_123.jpg"
        //public string? CoverImagePath { get; set; }

        public ICollection<ArticleCategory> Categories { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];

        public DateTimeOffset? PublicationDate { get; private set; } 
        
        public Error? Publish(DateTimeOffset publishedAt)
        {
            if (Status == ArticleStatus.Published)
                return ArticleErrors.ArticleAlreadyPublished(Id);

            PublicationDate = publishedAt;
            Status = ArticleStatus.Published;

            RaiseDomainEvent(new ArticlePublishedDomainEvent(Id, Title, (Guid)CreatedBy!, (DateTimeOffset)PublicationDate));

            return null;
        }
    }
}
