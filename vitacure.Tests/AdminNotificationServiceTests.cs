using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class AdminNotificationServiceTests
{
    [Fact]
    public async Task GetModuleAsync_Marks_Selected_Notification_As_Read()
    {
        await using var dbContext = CreateDbContext();
        var service = new AdminNotificationService(dbContext);

        await service.CreateAsync(new AdminNotificationCreateRequest
        {
            Title = "Yeni siparis olusturuldu",
            Summary = "Siparis feed testi",
            Body = "Siparis detay metni",
            Actor = "Test User",
            Source = "Storefront",
            CategoryKey = "orders"
        });

        var notificationId = await dbContext.AdminNotifications
            .Select(x => x.Id)
            .SingleAsync();

        var model = await service.GetModuleAsync("orders", notificationId);
        var notification = await dbContext.AdminNotifications.SingleAsync();

        Assert.NotNull(model.SelectedNotification);
        Assert.False(model.SelectedNotification!.IsUnread);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
    }

    [Fact]
    public async Task GetSummaryAsync_Returns_Unread_Count_And_Recent_Items()
    {
        await using var dbContext = CreateDbContext();
        var service = new AdminNotificationService(dbContext);

        await service.CreateAsync(new AdminNotificationCreateRequest
        {
            Title = "Yeni uye kaydi olustu",
            Summary = "Uyelik akisi",
            Body = "Uyelik detay",
            Actor = "Ayse",
            Source = "Auth",
            CategoryKey = "members",
            OccurredAt = new DateTime(2026, 4, 18, 10, 0, 0, DateTimeKind.Utc)
        });

        await service.CreateAsync(new AdminNotificationCreateRequest
        {
            Title = "Sepete urun eklendi",
            Summary = "Sepet akisi",
            Body = "Sepet detay",
            Actor = "Mehmet",
            Source = "Storefront",
            CategoryKey = "cart",
            OccurredAt = new DateTime(2026, 4, 18, 11, 0, 0, DateTimeKind.Utc)
        });

        var summary = await service.GetSummaryAsync(1);

        Assert.Equal(2, summary.UnreadCount);
        Assert.Single(summary.Items);
        Assert.Equal("Sepete urun eklendi", summary.Items[0].Title);
        Assert.Equal("Sepet", summary.Items[0].CategoryLabel);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
