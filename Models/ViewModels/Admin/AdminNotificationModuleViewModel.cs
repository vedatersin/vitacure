namespace vitacure.Models.ViewModels.Admin;

public class AdminNotificationModuleViewModel
{
    public AdminPageHeroViewModel Hero { get; set; } = new();
    public IReadOnlyList<AdminNotificationCategoryViewModel> Categories { get; set; } = Array.Empty<AdminNotificationCategoryViewModel>();
    public IReadOnlyList<AdminNotificationListItemViewModel> Notifications { get; set; } = Array.Empty<AdminNotificationListItemViewModel>();
    public AdminNotificationDetailViewModel? SelectedNotification { get; set; }
    public string ActiveCategoryKey { get; set; } = "all";
}

public class AdminNotificationCategoryViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsActive { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class AdminNotificationListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string IconClass { get; set; } = "fa-regular fa-bell";
    public string AccentClass { get; set; } = string.Empty;
    public bool IsUnread { get; set; }
    public bool IsSelected { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class AdminNotificationDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string IconClass { get; set; } = "fa-regular fa-bell";
    public string AccentClass { get; set; } = string.Empty;
    public bool IsUnread { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? TargetLabel { get; set; }
    public string? TargetUrl { get; set; }
}

public class AdminNotificationSummaryViewModel
{
    public int UnreadCount { get; set; }
    public IReadOnlyList<AdminNotificationSummaryItemViewModel> Items { get; set; } = Array.Empty<AdminNotificationSummaryItemViewModel>();
}

public class AdminNotificationSummaryItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string IconClass { get; set; } = "fa-regular fa-bell";
    public string AccentClass { get; set; } = string.Empty;
    public bool IsUnread { get; set; }
    public string Url { get; set; } = string.Empty;
}
