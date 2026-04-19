namespace vitacure.Models.ViewModels.Admin;

public class BrandListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int UsedCount { get; set; }
    public IReadOnlyList<BrandListItemViewModel> Brands { get; set; } = Array.Empty<BrandListItemViewModel>();
}

public class BrandListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}
