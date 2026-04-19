using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Account;

namespace vitacure.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext, IAdminNotificationService adminNotificationService)
    {
        _dbContext = dbContext;
        _adminNotificationService = adminNotificationService;
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
                        VariantLabel = item.VariantLabel,
                        Quantity = item.Quantity,
                        LineTotal = FormatPrice(item.LineTotal)
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderPlacementResultViewModel> PlaceOrderFromCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var actor = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

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
                Message = "Siparis olusturmak icin once hesabiniza bir adres ekleyin."
            };
        }

        var cartItems = await _dbContext.CustomerCartItems
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .Where(x => x.AppUserId == userId && x.Product != null && x.Product.IsActive)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            return new OrderPlacementResultViewModel
            {
                Message = "Siparis olusturmak icin sepette urun bulunmali."
            };
        }

        var stockValidationMessage = ValidateStockAvailability(cartItems);
        if (!string.IsNullOrWhiteSpace(stockValidationMessage))
        {
            return new OrderPlacementResultViewModel
            {
                Message = stockValidationMessage
            };
        }

        var order = new Order
        {
            AppUserId = userId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            TotalQuantity = cartItems.Sum(x => x.Quantity),
            TotalAmount = cartItems.Sum(x => x.Quantity * (x.ProductVariant != null ? x.ProductVariant.Price : x.Product!.Price)),
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            City = address.City,
            District = address.District,
            AddressLine = address.AddressLine,
            PostalCode = address.PostalCode,
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.Product!.Name,
                ProductSlug = item.Product.Slug,
                VariantLabel = item.ProductVariant != null ? $"{item.ProductVariant.GroupName}: {item.ProductVariant.OptionName}" : null,
                UnitPrice = item.ProductVariant != null ? item.ProductVariant.Price : item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Quantity * (item.ProductVariant != null ? item.ProductVariant.Price : item.Product.Price)
            }).ToList()
        };

        ApplyStockDecrements(cartItems);
        _dbContext.Orders.Add(order);
        _dbContext.CustomerCartItems.RemoveRange(cartItems);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _adminNotificationService.CreateAsync(new AdminNotificationCreateRequest
        {
            Title = "Yeni siparis olusturuldu",
            Summary = $"{order.OrderNumber} numarali siparis olustu ve odeme bekliyor.",
            Body = $"{actor ?? address.RecipientName} kullanicisi {order.TotalQuantity} urun iceren {order.OrderNumber} numarali siparisi olusturdu. Toplam tutar {FormatPrice(order.TotalAmount)} TL.",
            Actor = actor ?? address.RecipientName,
            Source = "Storefront",
            CategoryKey = "orders",
            TargetLabel = "Siparislere git",
            TargetUrl = "/admin/orders",
            OccurredAt = order.CreatedAt
        }, cancellationToken);

        return new OrderPlacementResultViewModel
        {
            IsSuccess = true,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Message = "Siparisiniz olusturuldu."
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

    private static string? ValidateStockAvailability(IReadOnlyList<CustomerCartItem> cartItems)
    {
        foreach (var item in cartItems)
        {
            if (item.Product is null)
            {
                continue;
            }

            if (item.ProductVariant is not null)
            {
                if (!item.ProductVariant.IsActive)
                {
                    return $"{item.Product.Name} icin secili varyant artik aktif degil.";
                }

                if (item.ProductVariant.Stock < item.Quantity)
                {
                    return $"{item.Product.Name} icin '{item.ProductVariant.OptionName}' varyantinda yeterli stok yok.";
                }

                continue;
            }

            if (item.Product.Stock < item.Quantity)
            {
                return $"{item.Product.Name} icin yeterli stok yok.";
            }
        }

        return null;
    }

    private static void ApplyStockDecrements(IReadOnlyList<CustomerCartItem> cartItems)
    {
        foreach (var item in cartItems)
        {
            if (item.Product is null)
            {
                continue;
            }

            item.Product.Stock = Math.Max(0, item.Product.Stock - item.Quantity);

            if (item.ProductVariant is not null)
            {
                item.ProductVariant.Stock = Math.Max(0, item.ProductVariant.Stock - item.Quantity);
            }
        }
    }

    private static string GetStatusLabel(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Beklemede",
            OrderStatus.Preparing => "Hazirlaniyor",
            OrderStatus.Completed => "Tamamlandi",
            OrderStatus.Cancelled => "Iptal",
            _ => "Bilinmiyor"
        };
    }
}
