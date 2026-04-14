using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class NotificationsController : Controller
{
    [HttpGet("/admin/notifications")]
    public IActionResult Index([FromQuery] string? category, [FromQuery] int? notificationId)
    {
        var model = BuildModel(category, notificationId);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Notifications/_NotificationContent.cshtml", model);
        }

        return View(model);
    }

    private AdminNotificationModuleViewModel BuildModel(string? category, int? notificationId)
    {
        var notifications = BuildNotifications();
        var activeCategory = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim().ToLowerInvariant();

        var filteredNotifications = activeCategory == "all"
            ? notifications
            : notifications.Where(notification => string.Equals(notification.CategoryKey, activeCategory, StringComparison.OrdinalIgnoreCase)).ToList();

        var selectedNotification = filteredNotifications.FirstOrDefault(notification => notification.Id == notificationId)
                                   ?? filteredNotifications.FirstOrDefault();

        var categories = BuildCategories(notifications, activeCategory, selectedNotification?.Id);

        return new AdminNotificationModuleViewModel
        {
            Hero = new AdminPageHeroViewModel
            {
                Eyebrow = "Bildirim Merkezi",
                Title = "Kullanici aksiyon bildirimleri",
                Description = "Storefront ve uye akislarindan gelen olaylari kategori bazli izleyip detayini sag panelde inceleyebilirsin.",
                IconClass = "fa-solid fa-bell",
                AsideTitle = "Modul notu",
                AsideText = "Bu ekran su an UI seviyesinde hazir. Gercek event kaydi, okunma durumu ve detay hedefleri backend omurgasina baglanacak.",
                Stats = new[]
                {
                    new AdminHeroStatViewModel { Label = "Toplam", Value = notifications.Count.ToString() },
                    new AdminHeroStatViewModel { Label = "Okunmamis", Value = notifications.Count(item => item.IsUnread).ToString() },
                    new AdminHeroStatViewModel { Label = "Aktif Filtre", Value = categories.First(item => item.IsActive).Label }
                },
                Breadcrumbs = new[]
                {
                    new AdminBreadcrumbItemViewModel { Label = "Dashboard", Url = "/admin/dashboard" },
                    new AdminBreadcrumbItemViewModel { Label = "Bildirimler", IsActive = true }
                }
            },
            Categories = categories,
            Notifications = filteredNotifications
                .Select(notification => new AdminNotificationListItemViewModel
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Summary = notification.Summary,
                    Actor = notification.Actor,
                    Source = notification.Source,
                    CategoryKey = notification.CategoryKey,
                    CategoryLabel = notification.CategoryLabel,
                    OccurredAt = notification.OccurredAt,
                    IconClass = notification.IconClass,
                    AccentClass = notification.AccentClass,
                    IsUnread = notification.IsUnread,
                    IsSelected = notification.Id == selectedNotification?.Id,
                    Url = BuildNotificationUrl(activeCategory, notification.Id)
                })
                .ToList(),
            SelectedNotification = selectedNotification is null
                ? null
                : new AdminNotificationDetailViewModel
                {
                    Id = selectedNotification.Id,
                    Title = selectedNotification.Title,
                    Body = selectedNotification.Body,
                    Actor = selectedNotification.Actor,
                    Source = selectedNotification.Source,
                    CategoryLabel = selectedNotification.CategoryLabel,
                    OccurredAt = selectedNotification.OccurredAt,
                    IconClass = selectedNotification.IconClass,
                    AccentClass = selectedNotification.AccentClass,
                    TargetLabel = selectedNotification.TargetLabel,
                    TargetUrl = selectedNotification.TargetUrl
                },
            ActiveCategoryKey = activeCategory
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private static List<NotificationSeed> BuildNotifications()
    {
        return
        [
            new(101, "Yeni siparis olusturuldu", "Siparis #VC-1042 olustu ve odeme bekliyor.", "Merve Kaya", "Storefront", "orders", "Yeni Siparis", DateTime.UtcNow.AddMinutes(-6), "fa-solid fa-bag-shopping", "is-order", true, "Merve Kaya kullanicisi 3 urunluk yeni bir siparis olusturdu. Siparis toplam tutari 2.349,90 TL ve odeme sonucu bekleniyor.", "Siparise git", "/admin/orders"),
            new(102, "Sepete urun eklendi", "Omega 3 urunu misafir sepetine eklendi.", "Misafir Oturum", "Storefront", "cart", "Sepet", DateTime.UtcNow.AddMinutes(-18), "fa-solid fa-cart-plus", "is-cart", true, "Misafir oturumdaki ziyaretci Omega 3 Premium urununu sepete ekledi. Login sonrasi merge akislarini izlemek icin bu olaya detay notu baglanacak.", "Siparis akislarina git", "/admin/orders"),
            new(103, "Urun favorilere eklendi", "Multivitamin kapsul favori listesine eklendi.", "Ece Demir", "Storefront", "favorites", "Favori", DateTime.UtcNow.AddMinutes(-37), "fa-solid fa-heart", "is-favorite", false, "Ece Demir kullanicisi Multivitamin Enerji urununu favorilerine ekledi. Bu olay gelecekte davranis bazli segmentasyon icin kullanilabilir.", "Urunlere git", "/admin/products"),
            new(104, "Yeni uye kaydi tamamlandi", "E-posta dogrulamali yeni hesap acildi.", "Okan Arslan", "Auth", "members", "Yeni Kayit", DateTime.UtcNow.AddHours(-2), "fa-solid fa-user-plus", "is-member", true, "Okan Arslan kullanicisi yeni hesap olusturdu ve e-posta dogrulamasini tamamlayarak storefront girisini acti.", "Kullanicilara git", "/admin/users"),
            new(105, "Yeni siparis tamamlandi", "Siparis #VC-1040 basariyla kapandi.", "Sena Yildiz", "Storefront", "orders", "Yeni Siparis", DateTime.UtcNow.AddHours(-5), "fa-solid fa-box-open", "is-order", false, "Sena Yildiz kullanicisinin siparisi kargoya hazir hale geldi ve durum tamamlandi olarak guncellendi.", "Siparise git", "/admin/orders"),
            new(106, "Sepet guncellendi", "Misafir kullanici adet bilgisini degistirdi.", "Misafir Oturum", "Storefront", "cart", "Sepet", DateTime.UtcNow.AddHours(-8), "fa-solid fa-cart-shopping", "is-cart", false, "Misafir kullanici sepetindeki Kolajen urunun adet bilgisini 1'den 2'ye cikardi.", "Sepet akislarina git", "/admin/orders"),
            new(107, "Favori listesi buyuyor", "Ayni urun farkli uyeler tarafindan favlandi.", "Segmentasyon", "Storefront", "favorites", "Favori", DateTime.UtcNow.AddDays(-1), "fa-solid fa-star", "is-favorite", false, "Son 24 saatte Magnezyum Complex urunu 11 farkli uye tarafindan favorilere eklendi.", "Urunlere git", "/admin/products"),
            new(108, "Yeni uye onboarding tamamladi", "Profil ve adres bilgileri tamamlandi.", "Elif Cakir", "Auth", "members", "Yeni Kayit", DateTime.UtcNow.AddDays(-1).AddHours(-3), "fa-solid fa-user-check", "is-member", false, "Elif Cakir kullanicisi adres kaydi ekleyip profilini tamamlayarak ilk siparis oncesi onboarding'i bitirdi.", "Kullanicilara git", "/admin/users")
        ];
    }

    private static List<AdminNotificationCategoryViewModel> BuildCategories(IReadOnlyList<NotificationSeed> notifications, string activeCategory, int? selectedNotificationId)
    {
        var definitions = new[]
        {
            new { Key = "all", Label = "Tumu" },
            new { Key = "orders", Label = "Yeni Siparis" },
            new { Key = "cart", Label = "Sepet" },
            new { Key = "favorites", Label = "Favori" },
            new { Key = "members", Label = "Yeni Kayit" }
        };

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

    private static string BuildNotificationUrl(string? category, int? notificationId)
    {
        var categoryValue = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim().ToLowerInvariant();
        return notificationId.HasValue
            ? $"/admin/notifications?category={Uri.EscapeDataString(categoryValue)}&notificationId={notificationId.Value}"
            : $"/admin/notifications?category={Uri.EscapeDataString(categoryValue)}";
    }

    private sealed record NotificationSeed(
        int Id,
        string Title,
        string Summary,
        string Actor,
        string Source,
        string CategoryKey,
        string CategoryLabel,
        DateTime OccurredAt,
        string IconClass,
        string AccentClass,
        bool IsUnread,
        string Body,
        string? TargetLabel,
        string? TargetUrl);
}
