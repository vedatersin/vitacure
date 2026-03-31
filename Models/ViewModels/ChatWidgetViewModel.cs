namespace vitacure.Models.ViewModels;

public class ChatWidgetViewModel
{
    public string Variant { get; set; } = "home";
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
    public string CompactBackLabel { get; set; } = string.Empty;
    public string CompactCategoryLabel { get; set; } = string.Empty;
    public string SearchFilterLabel { get; set; } = string.Empty;
    public string MainPlaceholder { get; set; } = string.Empty;
    public string FullscreenTitle { get; set; } = string.Empty;
    public string AddFileTitle { get; set; } = string.Empty;
    public string ChatModeLabel { get; set; } = string.Empty;
    public string SearchModeLabel { get; set; } = string.Empty;
    public string FileMenuDocumentLabel { get; set; } = string.Empty;
    public string FileMenuImageLabel { get; set; } = string.Empty;
    public string SearchPlaceholder { get; set; } = string.Empty;
    public string SearchPlaceholderLocked { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public IReadOnlyList<CategorySummaryViewModel> Categories { get; set; } = Array.Empty<CategorySummaryViewModel>();
    public IReadOnlyList<string> ExamplePrompts { get; set; } = Array.Empty<string>();
    public IDictionary<string, IReadOnlyList<string>> PromptPoolByCategory { get; set; } = new Dictionary<string, IReadOnlyList<string>>();
    public IDictionary<string, IReadOnlyList<string>> TagButtonsByCategory { get; set; } = new Dictionary<string, IReadOnlyList<string>>();
}
