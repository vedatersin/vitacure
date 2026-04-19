namespace vitacure.Models.ViewModels.Admin;

public class MediaLibraryViewModel
{
    public int TotalCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public IReadOnlyList<MediaAssetListItemViewModel> Items { get; set; } = Array.Empty<MediaAssetListItemViewModel>();
}

public class MediaAssetListItemViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string SizeLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UsageCount { get; set; }
}

public class MediaAssetUpdateInputModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? AltText { get; set; }
}
