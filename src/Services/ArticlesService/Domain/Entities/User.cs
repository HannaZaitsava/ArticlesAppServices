using ArticlesService.Domain.Entities.Base;
using Microsoft.AspNetCore.Identity;

namespace ArticlesService.Domain.Entities
{
    public class User : IdentityUser<Guid>, IAuditableEntity, ISoftDelete
    {
        public string? FirstName { get; set; } = null!;
        public string? LastName { get; set; } = null!;
        public DateTimeOffset? BirthDate { get; set; }
               
        public DateTimeOffset? AccountExpiryTime { get; set; }

        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
        
        public bool IsAdmin { get; set; }
        

        public ICollection<Article> Articles { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];

        public DateTimeOffset CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedOn { get; set; }

        public bool IsDeleted => DeletedOn.HasValue;
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset? DeletedOn { get; set; }       
    }
}
