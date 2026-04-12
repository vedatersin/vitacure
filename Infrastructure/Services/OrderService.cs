using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Account;

namespace vitacure.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountOrderSummaryViewModel>> GetOrderHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.AppUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AccountOrderSummaryViewModel
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                Status = GetStatusLabel(x.Status),
                TotalQuantity = x.TotalQuantity,
                TotalAmount = FormatPrice(x.TotalAmount),
                CreatedAt = x.CreatedAt,
                Items = x.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new AccountOrderItemViewModel
                    {
                        ProductName = item.ProductName,
                        ProductSlug = item.ProductSlug,
                        Quantity = item.Quantity,
                        LineTotal = FormatPrice(item.LineTotal)
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderPlacementResultViewModel> PlaceOrderFromCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.AppUserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (address is null)
        {
            return new OrderPlacementResultViewModel
            {
                Message = "Sipariş oluşturmak için önce hesabınıza bir adres ekleyin."
            };
        }

        var cartItems = await _dbContext.CustomerCartItems
            .Include(x => x.Product)
            .Where(x => x.AppUserId == userId && x.Product != null && x.Product.IsActive)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            return new OrderPlacementResultViewModel
            {
                Message = "Sipariş oluşturmak için sepette ürün bulunmalı."
            };
        }

        var order = new Order
        {
            AppUserId = userId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            TotalQuantity = cartItems.Sum(x => x.Quantity),
            TotalAmount = cartItems.Sum(x => x.Quantity * x.Product!.Price),
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            City = address.City,
            District = address.District,
            AddressLine = address.AddressLine,
            PostalCode = address.PostalCode,
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                ProductSlug = item.Product.Slug,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Quantity * item.Product.Price
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        _dbContext.CustomerCartItems.RemoveRange(cartItems);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OrderPlacementResultViewModel
        {
            IsSuccess = true,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Message = "Siparişiniz oluşturuldu."
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"VT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
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
