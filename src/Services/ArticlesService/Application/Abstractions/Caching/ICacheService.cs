using ArticlesService.Domain.Result;

namespace ArticlesService.Application.Abstractions.Caching
{
    public interface ICacheService
    {
        ValueTask<TResponse> GetOrSetAsync<TResponse>(
           string requestCacheKey,
           Func<CancellationToken, ValueTask<TResponse>> factory,
           int? requestExpirationSeconds,
           int? requestLocalCacheExpirationSeconds,
           bool bypassCacheRead,
           IEnumerable<string>? tags,
           CancellationToken ct = default);

        ///// <summary>
        ///// Метод кэширования для чистых типов (без Result)
        ///// </summary>
        ///// <typeparam name="TData"></typeparam>
        ///// <param name="requestCacheKey"></param>
        ///// <param name="factory"></param>
        ///// <param name="requestExpirationSeconds"></param>
        ///// <param name="requestLocalCacheExpirationSeconds"></param>
        ///// <param name="bypassCacheRead"></param>
        ///// <param name="tags"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //ValueTask<TData> GetOrSetAsync<TData>(
        //string requestCacheKey,
        //Func<CancellationToken, ValueTask<TData>> factory,
        //int? requestExpirationSeconds,
        //int? requestLocalCacheExpirationSeconds,
        //bool bypassCacheRead,
        //IEnumerable<string>? tags = null,
        //CancellationToken ct = default);

        ///// <summary>
        ///// Метод кэширования для Result Pattern
        ///// </summary>
        ///// <typeparam name="TData"></typeparam>
        ///// <typeparam name="TResponse"></typeparam>
        ///// <param name="requestCacheKey"></param>
        ///// <param name="factory"></param>
        ///// <param name="requestExpirationSeconds"></param>
        ///// <param name="requestLocalCacheExpirationSeconds"></param>
        ///// <param name="bypassCacheRead"></param>
        ///// <param name="tags"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //ValueTask<TResponse> GetOrSetResultAsync<TData, TResponse>(
        //    string requestCacheKey,
        //    Func<CancellationToken, ValueTask<TResponse>> factory,
        //    int? requestExpirationSeconds,
        //    int? requestLocalCacheExpirationSeconds,
        //    bool bypassCacheRead,
        //    IEnumerable<string>? tags = null,
        //    CancellationToken ct = default)
        //    where TResponse : IResult;

        ValueTask RemoveByTagAsync(string tag, CancellationToken ct = default);
        ValueTask RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken ct = default);
        ValueTask RemoveByKeyAsync(string key, CancellationToken ct = default);
        ValueTask RemoveByKeysAsync(IEnumerable<string> keys, CancellationToken ct = default);
    }
}
