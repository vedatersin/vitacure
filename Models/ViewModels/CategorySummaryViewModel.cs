namespace vitacure.Models.ViewModels;

public class CategorySummaryViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string PillCssClass { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionHtml { get; set; } = string.Empty;
    public string BackgroundImageUrl { get; set; } = string.Empty;
    public string BackgroundOverlay { get; set; } = string.Empty;
    public string BackgroundClass { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}
