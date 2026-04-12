using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class AdminDashboardServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_Returns_Correct_Metrics()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Uyku", Slug = "uyku", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Uncategorized", Slug = "uncategorized", Description = "B", IsActive = true });

        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Product A",
                Slug = "product-a",
                Description = "A",
                Price = 100m,
                Rating = 4.0m,
                ImageUrl = "/img/a.png",
                Stock = 10,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                Name = "Product B",
                Slug = "product-b",
                Description = "B",
                Price = 120m,
                Rating = 4.2m,
                ImageUrl = "/img/b.png",
                Stock = 10,
                CategoryId = 1,
                IsActive = false
            });

        dbContext.Users.AddRange(
            new AppUser
            {
                Id = 1,
                UserName = "customer@test.local",
                Email = "customer@test.local",
                FullName = "Customer User",
                AccountType = AccountType.Customer,
                IsActive = true
            },
            new AppUser
            {
                Id = 2,
                UserName = "admin@test.local",
                Email = "admin@test.local",
                FullName = "Admin User",
                AccountType = AccountType.BackOffice,
                IsActive = true
            });

        dbContext.Orders.Add(new Order
        {
            Id = 30,
            AppUserId = 1,
            OrderNumber = "VT-TEST-3000",
            Status = OrderStatus.Pending,
            TotalQuantity = 2,
            TotalAmount = 250m,
            RecipientName = "Customer User",
            PhoneNumber = "5550000000",
            City = "İstanbul",
            District = "Kadıköy",
            AddressLine = "Adres 1"
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminDashboardService(dbContext);

        var result = await service.GetDashboardAsync();

        Assert.Equal(1, result.ProductCount);
        Assert.Equal(1, result.CategoryCount);
        Assert.Equal(1, result.CustomerCount);
        Assert.Equal(1, result.BackOfficeUserCount);
        Assert.Equal(1, result.OrderCount);
        Assert.Equal(5, result.Cards.Count);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
