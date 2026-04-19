using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminNotificationService : IAdminNotificationService
{
    private static readonly IReadOnlyDictionary<string, NotificationCategoryDefinition> CategoryDefinitions =
        new Dictionary<string, NotificationCategoryDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["orders"] = new("orders", "Yeni Siparis", "fa-solid fa-bag-shopping", "is-order"),
            ["cart"] = new("cart", "Sepet", "fa-solid fa-cart-plus", "is-cart"),
            ["favorites"] = new("favorites", "Favori", "fa-solid fa-heart", "is-favorite"),
            ["members"] = new("members", "Yeni Kayit", "fa-solid fa-user-plus", "is-member"),
            ["auth"] = new("auth", "Auth", "fa-solid fa-shield-halved", "is-auth")
        };

    private readonly AppDbContext _dbContext;

    public AdminNotificationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminNotificationModuleViewModel> GetModuleAsync(
        string? category,
        int? notificationId,
        CancellationToken cancellationToken = default)
    {
        if (notificationId.HasValue)
        {
            await MarkAsReadAsync(notificationId.Value, cancellationToken);
        }

        var notifications = await _dbContext.AdminNotifications
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        var activeCategory = NormalizeCategory(category);
        var filteredNotifications = activeCategory == "all"
            ? notifications
            : notifications.Where(notification =>
                string.Equals(notification.CategoryKey, activeCategory, StringComparison.OrdinalIgnoreCase)).ToList();

        var selectedNotification = filteredNotifications.FirstOrDefault(notification => notification.Id == notificationId)
                                   ?? filteredNotifications.FirstOrDefault();
        var categories = BuildCategories(notifications, activeCategory, selectedNotification?.Id);

        return new AdminNotificationModuleViewModel
        {
            Hero = new AdminPageHeroViewModel
            {
                Eyebrow = "Bildirim Merkezi",
                Title = "Kullanici aksiyon bildirimleri",
                Description = "Storefront ve auth akislarindan gelen olaylari kategori bazli izleyip detayini sag panelde inceleyebilirsin.",
                IconClass = "fa-solid fa-bell",
                AsideTitle = "Backend durumu",
                AsideText = "Bu ekran artik kalici admin notification feed'i ile besleniyor. Bir bildirimin detayina girdiginde okunma durumu otomatik guncellenir.",
                Stats = new[]
                {
                    new AdminHeroStatViewModel { Label = "Toplam", Value = notifications.Count.ToString() },
                    new AdminHeroStatViewModel { Label = "Okunmamis", Value = notifications.Count(item => !item.IsRead).ToString() },
                    new AdminHeroStatViewModel { Label = "Aktif Filtre", Value = categories.First(item => item.IsActive).Label }
                },
                Breadcrumbs = new[]
                {
                    new AdminBreadcrumbItemViewModel { Label = "Dashboard", Url = "/admin/dashboard" },
                    new AdminBreadcrumbItemViewModel { Label = "Bildirimler", IsActive = true }
                }
            },
            Categories = categories,
            Notifications = filteredNotifications.Select(notification => BuildListItem(notification, selectedNotification?.Id)).ToList(),
            SelectedNotification = selectedNotification is null ? null : BuildDetailItem(selectedNotification),
            ActiveCategoryKey = activeCategory
        };
    }

    public async Task<AdminNotificationSummaryViewModel> GetSummaryAsync(int take = 4, CancellationToken cancellationToken = default)
    {
        var normalizedTake = Math.Max(1, take);
        var items = await _dbContext.AdminNotifications
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Take(normalizedTake)
            .ToListAsync(cancellationToken);

        var unreadCount = await _dbContext.AdminNotifications.CountAsync(x => !x.IsRead, cancellationToken);

        return new AdminNotificationSummaryViewModel
        {
            UnreadCount = unreadCount,
            Items = items.Select(BuildSummaryItem).ToList()
        };
    }

    public async Task CreateAsync(AdminNotificationCreateRequest request, CancellationToken cancellationToken = default)
    {
        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var categoryKey = NormalizeKnownCategory(request.CategoryKey);
        var summary = string.IsNullOrWhiteSpace(request.Summary) ? title : request.Summary.Trim();
        var body = string.IsNullOrWhiteSpace(request.Body) ? summary : request.Body.Trim();

        _dbContext.AdminNotifications.Add(new AdminNotification
        {
            Title = title,
            Summary = summary,
            Body = body,
            Actor = string.IsNullOrWhiteSpace(request.Actor) ? "Sistem" : request.Actor.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? "System" : request.Source.Trim(),
            CategoryKey = categoryKey,
            TargetLabel = string.IsNullOrWhiteSpace(request.TargetLabel) ? null : request.TargetLabel.Trim(),
            TargetUrl = string.IsNullOrWhiteSpace(request.TargetUrl) ? null : request.TargetUrl.Trim(),
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
            IsRead = false
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.AdminNotifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<AdminNotificationCategoryViewModel> BuildCategories(
        IReadOnlyCollection<AdminNotification> notifications,
        string activeCategory,
        int? selectedNotificationId)
    {
        var definitions = new List<(string Key, string Label)>
        {
            ("all", "Tumu")
        };

        definitions.AddRange(CategoryDefinitions.Values.Select(definition => (definition.Key, definition.Label)));

        return definitions
            .Select(definition => new AdminNotificationCategoryViewModel
            {
                Key = definition.Key,
                Label = definition.Label,
                Count = definition.Key == "all"
                    ? notifications.Count
                    : notifications.Count(item => string.Equals(item.CategoryKey, definition.Key, StringComparison.OrdinalIgnoreCase)),
                IsActive = string.Equals(activeCategory, definition.Key, StringComparison.OrdinalIgnoreCase),
                Url = BuildNotificationUrl(definition.Key, selectedNotificationId)
            })
            .ToList();
    }

    private static AdminNotificationListItemViewModel BuildListItem(AdminNotification notification, int? selectedNotificationId)
    {
        var definition = ResolveDefinition(notification.CategoryKey);

        return new AdminNotificationListItemViewModel
        {
            Id = notification.Id,
            Title = notification.Title,
            Summary = notification.Summary,
            Actor = notification.Actor,
            Source = notification.Source,
            CategoryKey = definition.Key,
            CategoryLabel = definition.Label,
            OccurredAt = notification.OccurredAt,
            IconClass = definition.IconClass,
            AccentClass = definition.AccentClass,
            IsUnread = !notification.IsRead,
            IsSelected = notification.Id == selectedNotificationId,
            Url = BuildNotificationUrl(definition.Key, notification.Id)
        };
    }

    private static AdminNotificationDetailViewModel BuildDetailItem(AdminNotification notification)
    {
        var definition = ResolveDefinition(notification.CategoryKey);

        return new AdminNotificationDetailViewModel
        {
            Id = notification.Id,
            Title = notification.Title,
            Summary = notification.Summary,
            Body = notification.Body,
            Actor = notification.Actor,
            Source = notification.Source,
            CategoryLabel = definition.Label,
            OccurredAt = notification.OccurredAt,
            IconClass = definition.IconClass,
            AccentClass = definition.AccentClass,
            IsUnread = !notification.IsRead,
            ReadAt = notification.ReadAt,
            TargetLabel = notification.TargetLabel,
            TargetUrl = notification.TargetUrl
        };
    }

    private static AdminNotificationSummaryItemViewModel BuildSummaryItem(AdminNotification notification)
    {
        var definition = ResolveDefinition(notification.CategoryKey);

        return new AdminNotificationSummaryItemViewModel
        {
            Id = notification.Id,
            Title = notification.Title,
            Source = notification.Source,
            CategoryLabel = definition.Label,
            OccurredAt = notification.OccurredAt,
            IconClass = definition.IconClass,
            AccentClass = definition.AccentClass,
            IsUnread = !notification.IsRead,
            Url = BuildNotificationUrl(definition.Key, notification.Id)
        };
    }

    private static NotificationCategoryDefinition ResolveDefinition(string? categoryKey)
    {
        if (!string.IsNullOrWhiteSpace(categoryKey) && CategoryDefinitions.TryGetValue(categoryKey.Trim(), out var definition))
        {
            return definition;
        }

        return new NotificationCategoryDefinition("auth", "Auth", "fa-solid fa-bell", "is-auth");
    }

    private static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "all";
        }

        var normalized = category.Trim().ToLowerInvariant();
        return normalized == "all" || CategoryDefinitions.ContainsKey(normalized)
            ? normalized
            : "all";
    }

    private static string NormalizeKnownCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "auth";
        }

        var normalized = category.Trim().ToLowerInvariant();
        return CategoryDefinitions.ContainsKey(normalized) ? normalized : "auth";
    }

    private static string BuildNotificationUrl(string? category, int? notificationId)
    {
        var categoryValue = NormalizeCategory(category);
        return notificationId.HasValue
            ? $"/admin/notifications?category={Uri.EscapeDataString(categoryValue)}&notificationId={notificationId.Value}"
            : $"/admin/notifications?category={Uri.EscapeDataString(categoryValue)}";
    }

    private sealed record NotificationCategoryDefinition(string Key, string Label, string IconClass, string AccentClass);
}
