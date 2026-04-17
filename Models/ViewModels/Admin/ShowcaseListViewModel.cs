namespace vitacure.Models.ViewModels.Admin;

public class ShowcaseListViewModel
{
    public string? SearchTerm { get; set; }
    public string StatusFilter { get; set; } = "all";
    public string HomeFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int HomeVisibleCount { get; set; }
    public IReadOnlyList<ShowcaseListItemViewModel> Showcases { get; set; } = Array.Empty<ShowcaseListItemViewModel>();
}

public class ShowcaseListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string BackgroundImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool ShowOnHome { get; set; }
    public int CategoryCount { get; set; }
    public int FeaturedProductCount { get; set; }
}
