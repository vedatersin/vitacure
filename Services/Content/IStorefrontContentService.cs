using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public interface IStorefrontContentService
{
    Task<HomeViewModel> GetHomePageContentAsync(CancellationToken cancellationToken = default);
    Task<ShowcaseViewModel?> GetShowcasePageContentAsync(string slug, string? categorySlug = null, CancellationToken cancellationToken = default);
    Task<CategoryViewModel?> GetCategoryPageContentAsync(string slug, string? tagSlug = null, CancellationToken cancellationToken = default);
    Task<ProductDetailViewModel?> GetProductDetailPageContentAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategorySummaryViewModel>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
