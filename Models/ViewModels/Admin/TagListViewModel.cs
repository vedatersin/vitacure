namespace vitacure.Models.ViewModels.Admin;

public class TagListViewModel
{
    public string? SearchTerm { get; set; }
    public string UsageFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int UsedCount { get; set; }
    public IReadOnlyList<TagListItemViewModel> Tags { get; set; } = Array.Empty<TagListItemViewModel>();
}

public class TagListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
