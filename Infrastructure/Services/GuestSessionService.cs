using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Account;
using vitacure.Models.ViewModels.Cart;

namespace vitacure.Infrastructure.Services;

public class GuestSessionService : IGuestSessionService
{
    private const string FavoriteCookieName = "vitacure_guest_favorites";
    private const string CartCookieName = "vitacure_guest_cart";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GuestSessionService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public IReadOnlyList<string> GetFavoriteProductSlugs()
    {
        return ReadFavorites();
    }

    public int GetFavoriteCount()
    {
        return ReadFavorites().Count;
    }

    public FavoriteToggleResultViewModel ToggleFavorite(string productSlug)
    {
        var favorites = ReadFavorites();
        if (string.IsNullOrWhiteSpace(productSlug))
        {
            return new FavoriteToggleResultViewModel
            {
                FavoriteCount = favorites.Count
            };
        }

        var normalizedSlug = productSlug.Trim();
        var removed = favorites.RemoveAll(x => string.Equals(x, normalizedSlug, StringComparison.OrdinalIgnoreCase)) > 0;
        var isFavorite = !removed;

        if (isFavorite)
        {
            favorites.Add(normalizedSlug);
        }

        WriteFavorites(favorites);

        return new FavoriteToggleResultViewModel
        {
            IsFavorite = isFavorite,
            FavoriteCount = favorites.Count
        };
    }

    public async Task<CartViewModel> GetCartAsync(CancellationToken cancellationToken = default)
    {
        var cartItems = ReadCartItems();
        if (cartItems.Count == 0)
        {
            return new CartViewModel();
        }

        var normalizedCart = cartItems
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductSlug) && x.Quantity > 0)
            .GroupBy(x => x.ProductSlug.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new GuestCartCookieItem
            {
                ProductSlug = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();

        var slugs = normalizedCart.Select(x => x.ProductSlug).ToList();
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(x => slugs.Contains(x.Slug) && x.IsActive)
            .ToDictionaryAsync(x => x.Slug, x => x, cancellationToken);

        var items = normalizedCart
            .Where(x => products.ContainsKey(x.ProductSlug))
            .Select(x =>
            {
                var product = products[x.ProductSlug];
                var lineTotal = product.Price * x.Quantity;

                return new CartItemViewModel
                {
                    ProductSlug = product.Slug,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    ProductHref = $"/urun/{product.Slug}",
                    Quantity = x.Quantity,
                    UnitPriceValue = product.Price,
                    UnitPrice = FormatPrice(product.Price),
                    LineTotalValue = lineTotal,
                    LineTotal = FormatPrice(lineTotal)
                };
            })
            .OrderByDescending(x => x.Quantity)
            .ThenBy(x => x.ProductName)
            .ToList();

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

    public int GetCartItemCount()
    {
        return ReadCartItems()
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductSlug) && x.Quantity > 0)
            .Sum(x => x.Quantity);
    }

    public async Task<CartMutationResultViewModel> AddCartItemAsync(string productSlug, int quantity = 1, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = productSlug?.Trim() ?? string.Empty;
        var normalizedQuantity = Math.Max(1, quantity);
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == normalizedSlug && x.IsActive, cancellationToken);

        if (product is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Urun bulunamadi."
            };
        }

        var items = ReadCartItems();
        var existingItem = items.FirstOrDefault(x => string.Equals(x.ProductSlug, normalizedSlug, StringComparison.OrdinalIgnoreCase));
        if (existingItem is null)
        {
            items.Add(new GuestCartCookieItem
            {
                ProductSlug = normalizedSlug,
                Quantity = normalizedQuantity
            });
            existingItem = items[^1];
        }
        else
        {
            existingItem.Quantity += normalizedQuantity;
        }

        WriteCartItems(items);
        return await BuildCartMutationResultAsync(items, existingItem.Quantity, "Urun sepete eklendi.", cancellationToken);
    }

    public async Task<CartMutationResultViewModel> UpdateCartQuantityAsync(string productSlug, int quantity, CancellationToken cancellationToken = default)
    {
        var items = ReadCartItems();
        var existingItem = items.FirstOrDefault(x => string.Equals(x.ProductSlug, productSlug, StringComparison.OrdinalIgnoreCase));
        if (existingItem is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Sepet urunu bulunamadi."
            };
        }

        if (quantity <= 0)
        {
            items.Remove(existingItem);
            WriteCartItems(items);
            return await BuildCartMutationResultAsync(items, 0, "Urun sepetten cikarildi.", cancellationToken);
        }

        existingItem.Quantity = quantity;
        WriteCartItems(items);
        return await BuildCartMutationResultAsync(items, existingItem.Quantity, "Sepet guncellendi.", cancellationToken);
    }

    public async Task<CartMutationResultViewModel> RemoveCartItemAsync(string productSlug, CancellationToken cancellationToken = default)
    {
        var items = ReadCartItems();
        var existingItem = items.FirstOrDefault(x => string.Equals(x.ProductSlug, productSlug, StringComparison.OrdinalIgnoreCase));
        if (existingItem is null)
        {
            return new CartMutationResultViewModel
            {
                Message = "Sepet urunu bulunamadi."
            };
        }

        items.Remove(existingItem);
        WriteCartItems(items);
        return await BuildCartMutationResultAsync(items, 0, "Urun sepetten cikarildi.", cancellationToken);
    }

    public async Task<bool> MergeIntoCustomerAccountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var favoriteSlugs = ReadFavorites();
        var cartItems = ReadCartItems()
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductSlug) && x.Quantity > 0)
            .ToList();

        if (favoriteSlugs.Count == 0 && cartItems.Count == 0)
        {
            return true;
        }

        var userExists = await _dbContext.Users.AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            return false;
        }

        var productSlugs = favoriteSlugs
            .Concat(cartItems.Select(x => x.ProductSlug))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var products = await _dbContext.Products
            .Where(x => productSlugs.Contains(x.Slug) && x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var favoriteSlug in favoriteSlugs)
        {
            var product = products.FirstOrDefault(x => string.Equals(x.Slug, favoriteSlug, StringComparison.OrdinalIgnoreCase));
            if (product is null)
            {
                continue;
            }

            var exists = await _dbContext.CustomerFavorites.AnyAsync(
                x => x.AppUserId == userId && x.ProductId == product.Id,
                cancellationToken);

            if (!exists)
            {
                _dbContext.CustomerFavorites.Add(new CustomerFavorite
                {
                    AppUserId = userId,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(x => string.Equals(x.Slug, cartItem.ProductSlug, StringComparison.OrdinalIgnoreCase));
            if (product is null)
            {
                continue;
            }

            var existingCartItem = await _dbContext.CustomerCartItems.FirstOrDefaultAsync(
                x => x.AppUserId == userId && x.ProductId == product.Id,
                cancellationToken);

            if (existingCartItem is null)
            {
                _dbContext.CustomerCartItems.Add(new CustomerCartItem
                {
                    AppUserId = userId,
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingCartItem.Quantity += cartItem.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        ClearCookies();
        return true;
    }

    private async Task<CartMutationResultViewModel> BuildCartMutationResultAsync(
        IReadOnlyList<GuestCartCookieItem> items,
        int itemQuantity,
        string message,
        CancellationToken cancellationToken)
    {
        var normalizedCart = items
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductSlug) && x.Quantity > 0)
            .GroupBy(x => x.ProductSlug.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new GuestCartCookieItem
            {
                ProductSlug = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();

        var slugs = normalizedCart.Select(x => x.ProductSlug).ToList();
        var prices = await _dbContext.Products
            .AsNoTracking()
            .Where(x => slugs.Contains(x.Slug) && x.IsActive)
            .ToDictionaryAsync(x => x.Slug, x => x.Price, cancellationToken);

        var cartCount = normalizedCart.Sum(x => x.Quantity);
        var totalAmount = normalizedCart.Sum(x => prices.TryGetValue(x.ProductSlug, out var price) ? price * x.Quantity : 0m);

        return new CartMutationResultViewModel
        {
            IsSuccess = true,
            Message = message,
            CartItemCount = cartCount,
            ItemQuantity = itemQuantity,
            CartTotalAmountValue = totalAmount,
            CartTotalAmount = FormatPrice(totalAmount)
        };
    }

    private List<string> ReadFavorites()
    {
        var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[FavoriteCookieName];
        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(cookieValue, JsonOptions)?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private void WriteFavorites(List<string> favorites)
    {
        var normalizedFavorites = favorites
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        WriteCookie(FavoriteCookieName, normalizedFavorites);
    }

    private List<GuestCartCookieItem> ReadCartItems()
    {
        var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[CartCookieName];
        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            return new List<GuestCartCookieItem>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<GuestCartCookieItem>>(cookieValue, JsonOptions)?
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductSlug) && x.Quantity > 0)
                .Select(x => new GuestCartCookieItem
                {
                    ProductSlug = x.ProductSlug.Trim(),
                    Quantity = x.Quantity
                })
                .ToList() ?? new List<GuestCartCookieItem>();
        }
        catch
        {
            return new List<GuestCartCookieItem>();
        }
    }

    private void WriteCartItems(List<GuestCartCookieItem> items)
    {
        var normalizedItems = items
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductSlug) && x.Quantity > 0)
            .GroupBy(x => x.ProductSlug.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new GuestCartCookieItem
            {
                ProductSlug = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();

        WriteCookie(CartCookieName, normalizedItems);
    }

    private void WriteCookie<T>(string name, T payload)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        var hasData = payload switch
        {
            IEnumerable<string> strings => strings.Any(),
            IEnumerable<GuestCartCookieItem> items => items.Any(),
            _ => true
        };

        if (!hasData)
        {
            context.Response.Cookies.Delete(name);
            return;
        }

        context.Response.Cookies.Append(name, JsonSerializer.Serialize(payload, JsonOptions), BuildCookieOptions());
    }

    private void ClearCookies()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        context.Response.Cookies.Delete(FavoriteCookieName);
        context.Response.Cookies.Delete(CartCookieName);
    }

    private static CookieOptions BuildCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        };
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
    }

    private sealed class GuestCartCookieItem
    {
        public string ProductSlug { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
