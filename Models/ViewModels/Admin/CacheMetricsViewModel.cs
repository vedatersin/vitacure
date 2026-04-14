namespace vitacure.Models.ViewModels.Admin;

public class CacheMetricsViewModel
{
    public int TotalLookups { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public int WriteCount { get; set; }
    public int EvictionCount { get; set; }
    public IReadOnlyList<string> RecentEvictedTags { get; set; } = Array.Empty<string>();
    public string StatusLabel { get; set; } = "Soğuk";
    public string Detail { get; set; } = "Henüz cache trafiği oluşmadı.";
    public string HitRateLabel { get; set; } = "%0";
}
