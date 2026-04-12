using vitacure.Domain.Entities;

namespace vitacure.Application.Abstractions;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
