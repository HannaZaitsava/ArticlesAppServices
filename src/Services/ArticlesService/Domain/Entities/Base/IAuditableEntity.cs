namespace ArticlesService.Domain.Entities.Base
{
    public interface IAuditableEntity
    {
        Guid CreatorId { get; set; }
        DateTimeOffset CreatedOn { get; set; }   
        Guid? LastModifiedBy { get; set; }
        DateTimeOffset? LastModifiedOn { get; set; }
    }
}
