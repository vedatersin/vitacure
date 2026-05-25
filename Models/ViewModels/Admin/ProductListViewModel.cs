using vitacure.Domain.Enums;

namespace vitacure.Models.ViewModels.Admin;

public class ProductListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public string StockFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int OutOfStockCount { get; set; }
    public IReadOnlyList<string> BrandOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> CategoryOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> TagOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> SalesChannelOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ProductSavedFilterViewModel> SavedFilters { get; set; } = Array.Empty<ProductSavedFilterViewModel>();
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public class ProductSavedFilterViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public IReadOnlyList<ProductFilterRuleViewModel> Filters { get; set; } = Array.Empty<ProductFilterRuleViewModel>();
}

public class ProductFilterRuleViewModel
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public IReadOnlyList<string> Values { get; set; } = Array.Empty<string>();
    public string? Value { get; set; }
}

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> SalesChannels { get; set; } = Array.Empty<string>();
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Stock { get; set; }
    public string StockSummary { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public ProductPublishingStatus Status { get; set; } = ProductPublishingStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int FeatureCount { get; set; }
    public int TagCount { get; set; }
    public int VariantCount { get; set; }
    public string VariantSummary { get; set; } = string.Empty;
    public bool HasDiscount => OldPrice.HasValue && OldPrice.Value > Price;
}
