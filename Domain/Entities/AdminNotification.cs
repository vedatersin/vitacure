namespace vitacure.Domain.Entities;

public class AdminNotification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string? TargetLabel { get; set; }
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
