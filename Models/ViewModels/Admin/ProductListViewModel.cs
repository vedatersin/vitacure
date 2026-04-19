namespace vitacure.Models.ViewModels.Admin;

public class ProductListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public string StockFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int OutOfStockCount { get; set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string StockSummary { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int FeatureCount { get; set; }
    public int TagCount { get; set; }
    public int VariantCount { get; set; }
    public string VariantSummary { get; set; } = string.Empty;
}
