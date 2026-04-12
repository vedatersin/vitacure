namespace vitacure.Models.ViewModels.Admin;

public class CategoryListViewModel
{
    public int TotalCount { get; set; }
    public int RootCount { get; set; }
    public int ActiveCount { get; set; }
    public IReadOnlyList<CategoryListItemViewModel> Categories { get; set; } = Array.Empty<CategoryListItemViewModel>();
}

public class CategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ParentName { get; set; } = "-";
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}
