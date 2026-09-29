namespace ArticlesService.Application.Common.Caching
{
    public interface ICacheInvalidationContext
    {
        IReadOnlySet<string> Tags { get; }
        void AddTag(string tag);
        void AddTags(IEnumerable<string> tags);
    }
}
