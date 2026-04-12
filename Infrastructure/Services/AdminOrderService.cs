using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminOrderService : IAdminOrderService
{
    private readonly AppDbContext _dbContext;

    public AdminOrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminOrderListViewModel> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.AppUser)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new AdminOrderListViewModel
        {
            TotalCount = orders.Count,
            PendingCount = orders.Count(x => x.Status == OrderStatus.Pending || x.Status == OrderStatus.Preparing),
            CompletedCount = orders.Count(x => x.Status == OrderStatus.Completed),
            TotalRevenue = orders.Sum(x => x.TotalAmount),
            Orders = orders.Select(order => new AdminOrderListItemViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.AppUser?.FullName ?? "-",
                CustomerEmail = order.AppUser?.Email ?? "-",
                Status = GetStatusLabel(order.Status),
                TotalQuantity = order.TotalQuantity,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            }).ToList()
        };
    }

    private static string GetStatusLabel(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Beklemede",
            OrderStatus.Preparing => "Hazırlanıyor",
            OrderStatus.Completed => "Tamamlandı",
            OrderStatus.Cancelled => "İptal",
            _ => "Bilinmiyor"
        };
    }
}
