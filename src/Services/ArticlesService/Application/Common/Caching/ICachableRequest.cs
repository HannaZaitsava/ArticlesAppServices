namespace ArticlesService.Application.Common.Caching
{
    public interface ICachableRequest
    {
        bool BypassCache => false; 
        IEnumerable<string>? CacheTags => [];
        int ExpirationSeconds => 20;
        int LocalCacheExpirationSeconds => 10;
        string GetCacheKeyMetadata();
    }    
}
