using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly IRedisConnectionStatusService _redisConnectionStatusService;
    private readonly ICacheObservabilityService _cacheObservabilityService;

    public AdminDashboardService(
        AppDbContext dbContext,
        IRedisConnectionStatusService redisConnectionStatusService,
        ICacheObservabilityService cacheObservabilityService)
    {
        _dbContext = dbContext;
        _redisConnectionStatusService = redisConnectionStatusService;
        _cacheObservabilityService = cacheObservabilityService;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var productCount = await _dbContext.Products.CountAsync(x => x.IsActive, cancellationToken);
        var categoryCount = await _dbContext.Categories.CountAsync(x => x.IsActive && x.Slug != "uncategorized", cancellationToken);
        var customerCount = await _dbContext.Users.CountAsync(x => x.IsActive && x.AccountType == AccountType.Customer, cancellationToken);
        var backOfficeUserCount = await _dbContext.Users.CountAsync(x => x.IsActive && x.AccountType == AccountType.BackOffice, cancellationToken);
        var orderCount = await _dbContext.Orders.CountAsync(cancellationToken);
        var redisStatus = await _redisConnectionStatusService.GetStatusAsync(cancellationToken);
        var cacheMetrics = _cacheObservabilityService.GetSnapshot();

        return new DashboardViewModel
        {
            ProductCount = productCount,
            CategoryCount = categoryCount,
            CustomerCount = customerCount,
            BackOfficeUserCount = backOfficeUserCount,
            OrderCount = orderCount,
            RedisStatus = redisStatus,
            CacheMetrics = cacheMetrics,
            Cards = new[]
            {
                new DashboardMetricCardViewModel
                {
                    Title = "Aktif Ürünler",
                    Value = productCount.ToString(),
                    Description = "Storefront tarafında listelenen ürün adedi."
                },
                new DashboardMetricCardViewModel
                {
                    Title = "Aktif Kategoriler",
                    Value = categoryCount.ToString(),
                    Description = "Dinamik route ile yayında olan kategori sayısı."
                },
                new DashboardMetricCardViewModel
                {
                    Title = "Müşteri Hesapları",
                    Value = customerCount.ToString(),
                    Description = "Alışveriş akışına giriş yapabilen üye sayısı."
                },
                new DashboardMetricCardViewModel
                {
                    Title = "Yönetim Hesapları",
                    Value = backOfficeUserCount.ToString(),
                    Description = "Admin veya editor rolündeki backoffice kullanıcı sayısı."
                },
                new DashboardMetricCardViewModel
                {
                    Title = "Siparişler",
                    Value = orderCount.ToString(),
                    Description = "Sepetten oluşturulan toplam sipariş sayısı."
                }
            }
        };
    }
}
