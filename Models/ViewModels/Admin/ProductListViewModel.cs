namespace vitacure.Models.ViewModels.Admin;

public class ProductListViewModel
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int OutOfStockCount { get; set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public int TagCount { get; set; }
}
