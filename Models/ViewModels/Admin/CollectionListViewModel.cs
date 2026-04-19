namespace vitacure.Models.ViewModels.Admin;

public class CollectionListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public string HomeFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int HomeVisibleCount { get; set; }
    public IReadOnlyList<CollectionListItemViewModel> Collections { get; set; } = Array.Empty<CollectionListItemViewModel>();
}

public class CollectionListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool ShowOnHome { get; set; }
    public int SortOrder { get; set; }
    public int ProductCount { get; set; }
}
