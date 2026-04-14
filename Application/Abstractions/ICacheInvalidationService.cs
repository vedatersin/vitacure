namespace vitacure.Application.Abstractions;

public interface ICacheInvalidationService
{
    Task InvalidateStorefrontAsync(CancellationToken cancellationToken = default);
    Task InvalidateCategoryAsync(CancellationToken cancellationToken = default);
    Task InvalidateProductAsync(CancellationToken cancellationToken = default);
}
