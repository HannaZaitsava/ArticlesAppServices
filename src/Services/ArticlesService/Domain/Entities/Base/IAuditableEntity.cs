namespace ArticlesService.Domain.Entities.Base
{
    public interface IAuditableEntity
    {
        Guid? CreatedBy { get; set; }

        DateTimeOffset CreatedOn { get; set; }
        

        Guid? LastModifiedBy { get; set; }

        DateTimeOffset? LastModifiedOn { get; set; }
    }
}
