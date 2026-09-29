namespace ArticlesService.Application.Common.Caching
{
    public class CacheInvalidationContext : ICacheInvalidationContext
    {
        private readonly HashSet<string> _tags = new();
        public IReadOnlySet<string> Tags => _tags;

        public void AddTag(string tag) => _tags.Add(tag);
        public void AddTags(IEnumerable<string> tags)
        {
            foreach (var tag in tags) _tags.Add(tag);
        }
    }
}
