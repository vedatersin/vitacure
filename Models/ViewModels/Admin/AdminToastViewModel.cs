namespace vitacure.Models.ViewModels.Admin;

public class AdminToastViewModel
{
    public string Type { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Details { get; set; } = Array.Empty<string>();
    public bool IsSticky { get; set; }
}
