namespace vitacure.Models.ViewModels;

public class ShowcaseSummaryViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string PillCssClass { get; set; } = string.Empty;
    public string BackgroundImageUrl { get; set; } = string.Empty;
    public bool IsDark { get; set; } = true;
}
