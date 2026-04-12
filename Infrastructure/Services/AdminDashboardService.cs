using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _dbContext;

    public AdminDashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var productCount = await _dbContext.Products.CountAsync(x => x.IsActive, cancellationToken);
        var categoryCount = await _dbContext.Categories.CountAsync(x => x.IsActive && x.Slug != "uncategorized", cancellationToken);
        var customerCount = await _dbContext.Users.CountAsync(x => x.IsActive && x.AccountType == AccountType.Customer, cancellationToken);
        var backOfficeUserCount = await _dbContext.Users.CountAsync(x => x.IsActive && x.AccountType == AccountType.BackOffice, cancellationToken);
        var orderCount = await _dbContext.Orders.CountAsync(cancellationToken);

        return new DashboardViewModel
        {
            ProductCount = productCount,
            CategoryCount = categoryCount,
            CustomerCount = customerCount,
            BackOfficeUserCount = backOfficeUserCount,
            OrderCount = orderCount,
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
