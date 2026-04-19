using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Cart;

namespace vitacure.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly AppDbContext _dbContext;

    public CartService(AppDbContext dbContext, IAdminNotificationService adminNotificationService)
    {
        _dbContext = dbContext;
        _adminNotificationService = adminNotificationService;
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
            .Include(x => x.ProductVariant)
            .Where(x => x.AppUserId == userId && x.Product != null && x.Product.IsActive)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new CartItemViewModel
            {
                ProductSlug = x.Product!.Slug,
                ProductName = x.Product.Name,
                VariantId = x.ProductVariantId,
                VariantLabel = x.ProductVariant != null ? $"{x.ProductVariant.GroupName}: {x.ProductVariant.OptionName}" : null,
                ProductImageUrl = x.Product.ImageUrl,
                ProductHref = $"/{x.Product.Slug}",
                Quantity = x.Quantity,
                UnitPriceValue = x.ProductVariant != null ? x.ProductVariant.Price : x.Product.Price,
                UnitPrice = FormatPrice(x.ProductVariant != null ? x.ProductVariant.Price : x.Product.Price),
                LineTotalValue = (x.ProductVariant != null ? x.ProductVariant.Price : x.Product.Price) * x.Quantity,
                LineTotal = FormatPrice((x.ProductVariant != null ? x.ProductVariant.Price : x.Product.Price) * x.Quantity)
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

    public async Task<CartMutationResultViewModel> AddItemAsync(int userId, string productSlug, int quantity = 1, int? variantId = null, CancellationToken cancellationToken = default)
    {
        var normalizedQuantity = Math.Max(1, quantity);
        var product = await _dbContext.Products
            .Include(x => x.ProductVariants)
            .FirstOrDefaultAsync(x => x.Slug == productSlug && x.IsActive, cancellationToken);

        if (product is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Urun bulunamadi."
            };
        }

        var variant = ResolveVariant(product, variantId);
        var resolvedVariantId = variant is null ? (int?)null : variant.Id;

        var cartItem = await _dbContext.CustomerCartItems
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.ProductId == product.Id && x.ProductVariantId == resolvedVariantId, cancellationToken);

        if (cartItem is null)
        {
            cartItem = new CustomerCartItem
            {
                AppUserId = userId,
                ProductId = product.Id,
                ProductVariantId = resolvedVariantId,
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
        await CreateCartNotificationAsync(
            userId,
            BuildProductDisplayName(product.Name, variant),
            normalizedQuantity,
            "Sepete urun eklendi",
            $"{BuildProductDisplayName(product.Name, variant)} urunu sepete eklendi.",
            $"{BuildProductDisplayName(product.Name, variant)} urunu musterinin sepetine {normalizedQuantity} adet eklendi.",
            cancellationToken);

        return await BuildMutationResultAsync(userId, cartItem.Quantity, "Urun sepete eklendi.", cancellationToken);
    }

    public async Task<CartMutationResultViewModel> UpdateQuantityAsync(int userId, string productSlug, int quantity, int? variantId = null, CancellationToken cancellationToken = default)
    {
        var cartItem = await _dbContext.CustomerCartItems
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.Product != null && x.Product.Slug == productSlug && x.ProductVariantId == variantId, cancellationToken);

        if (cartItem is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Sepet urunu bulunamadi."
            };
        }

        if (quantity <= 0)
        {
            _dbContext.CustomerCartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await BuildMutationResultAsync(userId, 0, "Urun sepetten cikarildi.", cancellationToken);
        }

        cartItem.Quantity = quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await CreateCartNotificationAsync(
            userId,
            BuildProductDisplayName(cartItem.Product!.Name, cartItem.ProductVariant),
            quantity,
            "Sepet guncellendi",
            $"{BuildProductDisplayName(cartItem.Product.Name, cartItem.ProductVariant)} icin adet bilgisi guncellendi.",
            $"{BuildProductDisplayName(cartItem.Product.Name, cartItem.ProductVariant)} urununun sepet adedi {quantity} olarak guncellendi.",
            cancellationToken);

        return await BuildMutationResultAsync(userId, cartItem.Quantity, "Sepet guncellendi.", cancellationToken);
    }

    public async Task<CartMutationResultViewModel> RemoveItemAsync(int userId, string productSlug, int? variantId, CancellationToken cancellationToken = default)
    {
        var cartItem = await _dbContext.CustomerCartItems
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.Product != null && x.Product.Slug == productSlug && x.ProductVariantId == variantId, cancellationToken);

        if (cartItem is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Sepet urunu bulunamadi."
            };
        }

        _dbContext.CustomerCartItems.Remove(cartItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildMutationResultAsync(userId, 0, "Urun sepetten cikarildi.", cancellationToken);
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
                Total = group.Sum(x => x.Quantity * (x.ProductVariant != null ? x.ProductVariant.Price : x.Product!.Price))
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

    private async Task CreateCartNotificationAsync(
        int userId,
        string productName,
        int quantity,
        string title,
        string summary,
        string body,
        CancellationToken cancellationToken)
    {
        var actor = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        await _adminNotificationService.CreateAsync(new AdminNotificationCreateRequest
        {
            Title = title,
            Summary = summary,
            Body = $"{actor ?? "Musteri"} kullanicisi {productName} urunu icin sepet aksiyonu gerceklestirdi. Guncel adet: {quantity}. {body}",
            Actor = actor ?? "Musteri",
            Source = "Storefront",
            CategoryKey = "cart",
            TargetLabel = "Siparis akislarina git",
            TargetUrl = "/admin/orders"
        }, cancellationToken);
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
    }

    private static ProductVariant? ResolveVariant(Product product, int? variantId)
    {
        if (!variantId.HasValue)
        {
            return null;
        }

        return product.ProductVariants.FirstOrDefault(x => x.Id == variantId.Value && x.IsActive);
    }

    private static string BuildProductDisplayName(string productName, ProductVariant? variant)
    {
        return variant is null ? productName : $"{productName} ({variant.GroupName}: {variant.OptionName})";
    }
}
