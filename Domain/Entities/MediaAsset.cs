namespace vitacure.Domain.Entities;

public class MediaAsset
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
