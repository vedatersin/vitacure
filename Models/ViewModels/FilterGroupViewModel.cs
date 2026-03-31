namespace vitacure.Models.ViewModels;

public class FilterGroupViewModel
{
    public string Group { get; set; } = string.Empty;
    public string PanelTitle { get; set; } = string.Empty;
    public string ClearLabel { get; set; } = string.Empty;
    public string ResultLabel { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public IReadOnlyList<string> SortOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<FilterOptionViewModel> Options { get; set; } = Array.Empty<FilterOptionViewModel>();
}

public class FilterOptionViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Count { get; set; } = string.Empty;
}
