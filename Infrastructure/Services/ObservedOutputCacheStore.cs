using Microsoft.AspNetCore.OutputCaching;
using vitacure.Application.Abstractions;

namespace vitacure.Infrastructure.Services;

public class ObservedOutputCacheStore : IOutputCacheStore
{
    private readonly IOutputCacheStore _innerStore;
    private readonly ICacheObservabilityService _cacheObservabilityService;

    public ObservedOutputCacheStore(IOutputCacheStore innerStore, ICacheObservabilityService cacheObservabilityService)
    {
        _innerStore = innerStore;
        _cacheObservabilityService = cacheObservabilityService;
    }

    public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        var cachedValue = await _innerStore.GetAsync(key, cancellationToken);
        _cacheObservabilityService.RecordLookup(cachedValue is not null);
        return cachedValue;
    }

    public async ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        await _innerStore.SetAsync(key, value, tags, validFor, cancellationToken);
        _cacheObservabilityService.RecordWrite();
    }

    public async ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        await _innerStore.EvictByTagAsync(tag, cancellationToken);
        _cacheObservabilityService.RecordTagEviction(tag);
    }
}
