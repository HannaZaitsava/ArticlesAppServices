namespace ArticlesService.Domain.Entities.Base
{
    public class BaseAuditableEntity : BaseEntity, IAuditableEntity, ISoftDelete
    {
        public Guid? CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }


        public bool IsDeleted => DeletedOn.HasValue;

        public Guid? DeletedBy { get; set; }

        public DateTimeOffset? DeletedOn { get; set; }


        public Guid? LastModifiedBy { get; set; }

        public DateTimeOffset? LastModifiedOn { get; set; }
    }
}
