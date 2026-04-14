using Microsoft.AspNetCore.OutputCaching;
using vitacure.Application.Abstractions;

namespace vitacure.Infrastructure.Services;

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IOutputCacheStore _outputCacheStore;

    public CacheInvalidationService(IOutputCacheStore outputCacheStore)
    {
        _outputCacheStore = outputCacheStore;
    }

    public async Task InvalidateStorefrontAsync(CancellationToken cancellationToken = default)
    {
        await _outputCacheStore.EvictByTagAsync("storefront", cancellationToken);
    }

    public async Task InvalidateCategoryAsync(CancellationToken cancellationToken = default)
    {
        await _outputCacheStore.EvictByTagAsync("category", cancellationToken);
        await InvalidateStorefrontAsync(cancellationToken);
    }

    public async Task InvalidateProductAsync(CancellationToken cancellationToken = default)
    {
        await _outputCacheStore.EvictByTagAsync("product", cancellationToken);
        await InvalidateCategoryAsync(cancellationToken);
    }
}
