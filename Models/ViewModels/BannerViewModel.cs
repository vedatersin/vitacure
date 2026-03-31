namespace vitacure.Models.ViewModels;

public class BannerViewModel
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = "#";
    public string? Gradient { get; set; }
    public string? TextColor { get; set; }
    public string? Title { get; set; }
}
