using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class AdminOrderServiceTests
{
    [Fact]
    public async Task GetOrdersAsync_Returns_Order_Metrics_And_Items()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Users.Add(new AppUser
        {
            Id = 1,
            UserName = "customer@test.local",
            Email = "customer@test.local",
            FullName = "Customer User",
            AccountType = AccountType.Customer,
            IsActive = true
        });

        dbContext.Orders.AddRange(
            new Order
            {
                Id = 1,
                AppUserId = 1,
                OrderNumber = "VT-TEST-0001",
                Status = OrderStatus.Pending,
                TotalQuantity = 2,
                TotalAmount = 299m,
                RecipientName = "Customer User",
                PhoneNumber = "5551111111",
                City = "İstanbul",
                District = "Kadıköy",
                AddressLine = "Adres 1"
            },
            new Order
            {
                Id = 2,
                AppUserId = 1,
                OrderNumber = "VT-TEST-0002",
                Status = OrderStatus.Completed,
                TotalQuantity = 1,
                TotalAmount = 159m,
                RecipientName = "Customer User",
                PhoneNumber = "5551111111",
                City = "İstanbul",
                District = "Kadıköy",
                AddressLine = "Adres 1"
            });

        await dbContext.SaveChangesAsync();

        var service = new AdminOrderService(dbContext);

        var result = await service.GetOrdersAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PendingCount);
        Assert.Equal(1, result.CompletedCount);
        Assert.Equal(458m, result.TotalRevenue);
        Assert.Equal("VT-TEST-0002", result.Orders[0].OrderNumber);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
