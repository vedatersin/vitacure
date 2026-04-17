namespace vitacure.Models.ViewModels;

public class ShowcaseViewModel
{
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalPath { get; set; } = "/";
    public ChatWidgetViewModel ChatWidget { get; set; } = new();
    public ShowcaseSummaryViewModel Showcase { get; set; } = new();
    public string DescriptionHtml { get; set; } = string.Empty;
    public IReadOnlyList<CategoryTagViewModel> Tags { get; set; } = Array.Empty<CategoryTagViewModel>();
    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; set; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyList<ProductCardViewModel> ProductGrid { get; set; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyList<ShowcaseFilterGroupViewModel> Filters { get; set; } = Array.Empty<ShowcaseFilterGroupViewModel>();
    public string ResultLabel { get; set; } = string.Empty;
    public string SortLabel { get; set; } = "Sirala:";
    public IReadOnlyList<string> SortOptions { get; set; } = Array.Empty<string>();
    public string? ActiveCategorySlug { get; set; }
}

public class ShowcaseFilterGroupViewModel
{
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<CategoryTagViewModel> Options { get; set; } = Array.Empty<CategoryTagViewModel>();
}
