namespace ArticlesService.Application.Abstractions.Caching
{
    public interface ICacheKeyBuilder
    {
        string Build(string contextName, string metadata);
    }
}