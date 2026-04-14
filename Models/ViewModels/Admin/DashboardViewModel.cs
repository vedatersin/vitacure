namespace vitacure.Models.ViewModels.Admin;

public class DashboardViewModel
{
    public int ProductCount { get; set; }
    public int CategoryCount { get; set; }
    public int CustomerCount { get; set; }
    public int BackOfficeUserCount { get; set; }
    public int OrderCount { get; set; }
    public RedisConnectionStatusViewModel RedisStatus { get; set; } = new();
    public CacheMetricsViewModel CacheMetrics { get; set; } = new();
    public IReadOnlyList<DashboardMetricCardViewModel> Cards { get; set; } = Array.Empty<DashboardMetricCardViewModel>();
}

public class DashboardMetricCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
