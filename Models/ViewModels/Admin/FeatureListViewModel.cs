namespace vitacure.Models.ViewModels.Admin;

public class FeatureListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int UsedCount { get; set; }
    public IReadOnlyList<FeatureListItemViewModel> Features { get; set; } = Array.Empty<FeatureListItemViewModel>();
}

public class FeatureListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? OptionsPreview { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}
