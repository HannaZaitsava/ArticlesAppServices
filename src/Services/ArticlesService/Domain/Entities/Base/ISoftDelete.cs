namespace ArticlesService.Domain.Entities.Base
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; }

        Guid? DeletedBy { get; set; }

        DateTimeOffset? DeletedOn { get; set; }
    }
}
