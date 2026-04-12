using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Cart;

namespace vitacure.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _dbContext;

    public CartService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> GetCartItemCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerCartItems
            .AsNoTracking()
            .Where(x => x.AppUserId == userId)
            .SumAsync(x => x.Quantity, cancellationToken);
    }

    public async Task<CartViewModel?> GetCartAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);

        if (!userExists)
        {
            return null;
        }

        var items = await _dbContext.CustomerCartItems
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.AppUserId == userId && x.Product != null && x.Product.IsActive)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new CartItemViewModel
            {
                ProductSlug = x.Product!.Slug,
                ProductName = x.Product.Name,
                ProductImageUrl = x.Product.ImageUrl,
                ProductHref = $"/urun/{x.Product.Slug}",
                Quantity = x.Quantity,
                UnitPriceValue = x.Product.Price,
                UnitPrice = FormatPrice(x.Product.Price),
                LineTotalValue = x.Product.Price * x.Quantity,
                LineTotal = FormatPrice(x.Product.Price * x.Quantity)
            })
            .ToListAsync(cancellationToken);

        var totalQuantity = items.Sum(x => x.Quantity);
        var totalAmount = items.Sum(x => x.LineTotalValue);

        return new CartViewModel
        {
            Items = items,
            TotalQuantity = totalQuantity,
            TotalAmountValue = totalAmount,
            TotalAmount = FormatPrice(totalAmount)
        };
    }

    public async Task<CartMutationResultViewModel> AddItemAsync(int userId, string productSlug, int quantity = 1, CancellationToken cancellationToken = default)
    {
        var normalizedQuantity = Math.Max(1, quantity);
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Slug == productSlug && x.IsActive, cancellationToken);

        if (product is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Ürün bulunamadı."
            };
        }

        var cartItem = await _dbContext.CustomerCartItems
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.ProductId == product.Id, cancellationToken);

        if (cartItem is null)
        {
            cartItem = new CustomerCartItem
            {
                AppUserId = userId,
                ProductId = product.Id,
                Quantity = normalizedQuantity,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.CustomerCartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += normalizedQuantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildMutationResultAsync(userId, cartItem.Quantity, "Ürün sepete eklendi.", cancellationToken);
    }

    public async Task<CartMutationResultViewModel> UpdateQuantityAsync(int userId, string productSlug, int quantity, CancellationToken cancellationToken = default)
    {
        var cartItem = await _dbContext.CustomerCartItems
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.Product != null && x.Product.Slug == productSlug, cancellationToken);

        if (cartItem is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Sepet ürünü bulunamadı."
            };
        }

        if (quantity <= 0)
        {
            _dbContext.CustomerCartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await BuildMutationResultAsync(userId, 0, "Ürün sepetten çıkarıldı.", cancellationToken);
        }

        cartItem.Quantity = quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildMutationResultAsync(userId, cartItem.Quantity, "Sepet güncellendi.", cancellationToken);
    }

    public async Task<CartMutationResultViewModel> RemoveItemAsync(int userId, string productSlug, CancellationToken cancellationToken = default)
    {
        var cartItem = await _dbContext.CustomerCartItems
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.Product != null && x.Product.Slug == productSlug, cancellationToken);

        if (cartItem is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Sepet ürünü bulunamadı."
            };
        }

        _dbContext.CustomerCartItems.Remove(cartItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildMutationResultAsync(userId, 0, "Ürün sepetten çıkarıldı.", cancellationToken);
    }

    private async Task<CartMutationResultViewModel> BuildMutationResultAsync(int userId, int itemQuantity, string message, CancellationToken cancellationToken)
    {
        var cartSummary = await _dbContext.CustomerCartItems
            .AsNoTracking()
            .Where(x => x.AppUserId == userId)
            .GroupBy(x => 1)
            .Select(group => new
            {
                Count = group.Sum(x => x.Quantity),
                Total = group.Sum(x => x.Quantity * x.Product!.Price)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new CartMutationResultViewModel
        {
            IsSuccess = true,
            Message = message,
            CartItemCount = cartSummary?.Count ?? 0,
            ItemQuantity = itemQuantity,
            CartTotalAmountValue = cartSummary?.Total ?? 0m,
            CartTotalAmount = FormatPrice(cartSummary?.Total ?? 0m)
        };
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
    }
}
