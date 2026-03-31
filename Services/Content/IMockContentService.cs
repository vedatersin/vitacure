using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public interface IMockContentService
{
    HomeViewModel GetHomePageContent();
    CategoryViewModel? GetCategoryPageContent(string slug);
    IReadOnlyList<CategorySummaryViewModel> GetCategories();
}
