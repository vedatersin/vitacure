namespace vitacure.Models.ViewModels;

public class CategoryViewModel
{
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalPath { get; set; } = "/";
    public CategorySummaryViewModel Category { get; set; } = new();
    public ChatWidgetViewModel ChatWidget { get; set; } = new();
    public BannerViewModel HeroBanner { get; set; } = new();
    public IReadOnlyList<FilterGroupViewModel> Filters { get; set; } = Array.Empty<FilterGroupViewModel>();
    public IReadOnlyList<ProductCardViewModel> CoverflowProducts { get; set; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyList<ProductCardViewModel> ProductGrid { get; set; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = Array.Empty<BreadcrumbItemViewModel>();
    public string ResultLabel { get; set; } = string.Empty;
    public string SortLabel { get; set; } = string.Empty;
    public IReadOnlyList<string> SortOptions { get; set; } = Array.Empty<string>();
    public string? ActiveTagSlug { get; set; }
    public string ActiveTagLabel { get; set; } = "Tümü";
    public IReadOnlyList<CategoryTagViewModel> AvailableTags { get; set; } = Array.Empty<CategoryTagViewModel>();
}

public class BreadcrumbItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CategoryTagViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
