using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class GuestSessionServiceTests
{
    [Fact]
    public async Task AddCartItemAsync_And_ToggleFavorite_Persist_In_Guest_Cookies()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        await dbContext.SaveChangesAsync();

        var firstContext = new DefaultHttpContext();
        var firstService = CreateGuestSessionService(dbContext, firstContext);

        var favoriteResult = firstService.ToggleFavorite("daily-multivitamin");
        var cartResult = await firstService.AddCartItemAsync("daily-multivitamin", 2);

        var secondContext = CreateContextFromResponseCookies(firstContext);
        var secondService = CreateGuestSessionService(dbContext, secondContext);
        var cart = await secondService.GetCartAsync();

        Assert.True(favoriteResult.IsFavorite);
        Assert.Equal(1, favoriteResult.FavoriteCount);
        Assert.True(cartResult.IsSuccess);
        Assert.Equal(2, cartResult.CartItemCount);
        Assert.Equal(1, secondService.GetFavoriteCount());
        Assert.Equal(2, secondService.GetCartItemCount());
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.TotalQuantity);
    }

    [Fact]
    public async Task MergeIntoCustomerAccountAsync_Transfers_Guest_Data_And_Clears_Cookies()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerFavorites.Add(new CustomerFavorite
        {
            AppUserId = 1,
            ProductId = 1
        });
        dbContext.CustomerCartItems.Add(new CustomerCartItem
        {
            AppUserId = 1,
            ProductId = 1,
            Quantity = 1
        });
        await dbContext.SaveChangesAsync();

        var context = new DefaultHttpContext();
        SetCookie(context, "vitacure_guest_favorites", JsonSerializer.Serialize(new[] { "daily-multivitamin" }), encodeValue: true);
        SetCookie(context, "vitacure_guest_cart", JsonSerializer.Serialize(new[]
        {
            new
            {
                ProductSlug = "daily-multivitamin",
                Quantity = 2
            }
        }), encodeValue: true);

        var service = CreateGuestSessionService(dbContext, context);

        var merged = await service.MergeIntoCustomerAccountAsync(1);

        var favoriteCount = await dbContext.CustomerFavorites.CountAsync(x => x.AppUserId == 1);
        var cartItem = await dbContext.CustomerCartItems.FirstAsync(x => x.AppUserId == 1 && x.ProductId == 1);

        Assert.True(merged);
        Assert.Equal(1, favoriteCount);
        Assert.Equal(3, cartItem.Quantity);
        Assert.Contains("vitacure_guest_favorites=", context.Response.Headers["Set-Cookie"].ToString());
        Assert.Contains("vitacure_guest_cart=", context.Response.Headers["Set-Cookie"].ToString());
    }

    [Fact]
    public async Task AddCartItemAsync_Persists_Variant_In_Guest_Cookie_And_Cart()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.ProductVariants.Add(new ProductVariant
        {
            Id = 21,
            ProductId = 1,
            GroupName = "Boyut",
            OptionName = "60 Tablet",
            Price = 299m,
            Stock = 10,
            SortOrder = 0,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var context = new DefaultHttpContext();
        var service = CreateGuestSessionService(dbContext, context);

        var result = await service.AddCartItemAsync("daily-multivitamin", 2, 21);
        var roundtripContext = CreateContextFromResponseCookies(context);
        var roundtripService = CreateGuestSessionService(dbContext, roundtripContext);
        var cart = await roundtripService.GetCartAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(cart.Items);
        Assert.Equal(21, cart.Items[0].VariantId);
        Assert.Equal("Boyut: 60 Tablet", cart.Items[0].VariantLabel);
        Assert.Equal("598,00", cart.TotalAmount);
    }

    private static GuestSessionService CreateGuestSessionService(AppDbContext dbContext, HttpContext httpContext)
    {
        return new GuestSessionService(
            dbContext,
            new HttpContextAccessor
            {
                HttpContext = httpContext
            },
            new AdminNotificationService(dbContext));
    }

    private static DefaultHttpContext CreateContextFromResponseCookies(DefaultHttpContext source)
    {
        var context = new DefaultHttpContext();
        foreach (var setCookieHeader in source.Response.Headers["Set-Cookie"].ToArray())
        {
            if (string.IsNullOrWhiteSpace(setCookieHeader))
            {
                continue;
            }

            var cookiePair = setCookieHeader.Split(';', 2)[0];
            var separatorIndex = cookiePair.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = cookiePair[..separatorIndex];
            var value = cookiePair[(separatorIndex + 1)..];
            SetCookie(context, name, value);
        }

        return context;
    }

    private static void SetCookie(HttpContext context, string name, string value, bool encodeValue = false)
    {
        var existingHeader = context.Request.Headers.Cookie.ToString();
        var cookieEntry = $"{name}={(encodeValue ? Uri.EscapeDataString(value) : value)}";
        context.Request.Headers.Cookie = string.IsNullOrWhiteSpace(existingHeader)
            ? cookieEntry
            : $"{existingHeader}; {cookieEntry}";
    }

    private static void SeedUserAndProduct(AppDbContext dbContext)
    {
        dbContext.Users.Add(new AppUser
        {
            Id = 1,
            UserName = "customer@test.local",
            Email = "customer@test.local",
            FullName = "Test User",
            AccountType = AccountType.Customer,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        });

        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku",
            Slug = "uyku",
            Description = "A",
            IsActive = true
        });

        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Daily Multivitamin",
            Slug = "daily-multivitamin",
            Description = "A",
            Price = 199m,
            Rating = 4.8m,
            ImageUrl = "/img/product.png",
            Stock = 15,
            CategoryId = 1,
            IsActive = true
        });
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
