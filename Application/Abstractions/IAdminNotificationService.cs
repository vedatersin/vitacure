using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminNotificationService
{
    Task<AdminNotificationModuleViewModel> GetModuleAsync(string? category, int? notificationId, CancellationToken cancellationToken = default);
    Task<AdminNotificationSummaryViewModel> GetSummaryAsync(int take = 4, CancellationToken cancellationToken = default);
    Task CreateAsync(AdminNotificationCreateRequest request, CancellationToken cancellationToken = default);
}

public class AdminNotificationCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string? TargetLabel { get; set; }
    public string? TargetUrl { get; set; }
    public DateTime? OccurredAt { get; set; }
}
