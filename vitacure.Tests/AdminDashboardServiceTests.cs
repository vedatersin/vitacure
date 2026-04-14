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

        var service = new AdminDashboardService(
            dbContext,
            new FakeRedisConnectionStatusService(),
            new FakeCacheObservabilityService());

        var result = await service.GetDashboardAsync();

        Assert.Equal(1, result.ProductCount);
        Assert.Equal(1, result.CategoryCount);
        Assert.Equal(1, result.CustomerCount);
        Assert.Equal(1, result.BackOfficeUserCount);
        Assert.Equal(1, result.OrderCount);
        Assert.Equal(5, result.Cards.Count);
        Assert.Equal("Bağlandı", result.RedisStatus.StatusLabel);
        Assert.Equal("%75", result.CacheMetrics.HitRateLabel);
        Assert.Equal(4, result.CacheMetrics.TotalLookups);
        Assert.Equal(1, result.CacheMetrics.EvictionCount);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeRedisConnectionStatusService : Application.Abstractions.IRedisConnectionStatusService
    {
        public Task<vitacure.Models.ViewModels.Admin.RedisConnectionStatusViewModel> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new vitacure.Models.ViewModels.Admin.RedisConnectionStatusViewModel
            {
                IsConfigured = true,
                IsConnected = true,
                StatusLabel = "Bağlandı",
                Detail = "Test bağlantısı başarılı."
            });
        }
    }

    private sealed class FakeCacheObservabilityService : Application.Abstractions.ICacheObservabilityService
    {
        public vitacure.Models.ViewModels.Admin.CacheMetricsViewModel GetSnapshot()
        {
            return new vitacure.Models.ViewModels.Admin.CacheMetricsViewModel
            {
                TotalLookups = 4,
                HitCount = 3,
                MissCount = 1,
                WriteCount = 2,
                EvictionCount = 1,
                HitRateLabel = "%75",
                StatusLabel = "Isındı",
                Detail = "Toplam 4 okuma, 3 hit, 1 miss, 2 yazma ve 1 invalidation gözlemlendi.",
                RecentEvictedTags = new[] { "product" }
            };
        }

        public void RecordLookup(bool hit)
        {
        }

        public void RecordWrite()
        {
        }

        public void RecordTagEviction(string tag)
        {
        }
    }
}
