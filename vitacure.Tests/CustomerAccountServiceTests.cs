using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Account;

namespace vitacure.Tests;

public class CustomerAccountServiceTests
{
    [Fact]
    public async Task ToggleFavoriteAsync_Adds_And_Removes_Favorite()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var added = await service.ToggleFavoriteAsync(1, "daily-multivitamin");
        var removed = await service.ToggleFavoriteAsync(1, "daily-multivitamin");

        Assert.True(added.IsFavorite);
        Assert.Equal(1, added.FavoriteCount);
        Assert.False(removed.IsFavorite);
        Assert.Equal(0, removed.FavoriteCount);
    }

    [Fact]
    public async Task AddAddressAsync_Clears_Previous_Default_When_New_Default_Is_Added()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerAddresses.Add(new CustomerAddress
        {
            Id = 10,
            AppUserId = 1,
            Title = "Ev",
            RecipientName = "Test User",
            PhoneNumber = "5551112233",
            City = "İstanbul",
            District = "Kadıköy",
            AddressLine = "Adres 1",
            IsDefault = true
        });
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var created = await service.AddAddressAsync(1, new AddressFormViewModel
        {
            Title = "Ofis",
            RecipientName = "Test User",
            PhoneNumber = "5559998877",
            City = "İstanbul",
            District = "Beşiktaş",
            AddressLine = "Adres 2",
            PostalCode = "34000",
            IsDefault = true
        });

        var addresses = await dbContext.CustomerAddresses
            .Where(x => x.AppUserId == 1)
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.True(created);
        Assert.Equal(2, addresses.Count);
        Assert.False(addresses[0].IsDefault);
        Assert.True(addresses[1].IsDefault);
    }

    [Fact]
    public async Task GetDashboardAsync_Returns_Favorites_And_Addresses()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerFavorites.Add(new CustomerFavorite
        {
            AppUserId = 1,
            ProductId = 1
        });
        dbContext.CustomerAddresses.Add(new CustomerAddress
        {
            Id = 10,
            AppUserId = 1,
            Title = "Ev",
            RecipientName = "Test User",
            PhoneNumber = "5551112233",
            City = "İstanbul",
            District = "Kadıköy",
            AddressLine = "Adres 1",
            IsDefault = true
        });
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var result = await service.GetDashboardAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.FavoriteCount);
        Assert.Equal(1, result.AddressCount);
        Assert.Equal("Daily Multivitamin", result.FavoriteProducts[0].Name);
        Assert.Equal("Ev", result.Addresses[0].Title);
    }

    [Fact]
    public async Task GetDashboardAsync_Returns_Order_History_When_Order_Exists()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.Orders.Add(new Order
        {
            Id = 15,
            AppUserId = 1,
            OrderNumber = "VT-TEST-1500",
            Status = OrderStatus.Pending,
            TotalQuantity = 2,
            TotalAmount = 398m,
            RecipientName = "Test User",
            PhoneNumber = "5551112233",
            City = "İstanbul",
            District = "Kadıköy",
            AddressLine = "Adres 1",
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = 16,
                    ProductId = 1,
                    ProductName = "Daily Multivitamin",
                    ProductSlug = "daily-multivitamin",
                    UnitPrice = 199m,
                    Quantity = 2,
                    LineTotal = 398m
                }
            }
        });
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var result = await service.GetDashboardAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.OrderCount);
        Assert.Equal("VT-TEST-1500", result.Orders[0].OrderNumber);
        Assert.Equal("Beklemede", result.Orders[0].Status);
    }

    [Fact]
    public async Task CartService_AddItemAsync_Persists_Item_And_Increments_Count()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        await dbContext.SaveChangesAsync();

        var service = new CartService(dbContext);

        var firstAdd = await service.AddItemAsync(1, "daily-multivitamin");
        var secondAdd = await service.AddItemAsync(1, "daily-multivitamin", 2);
        var cart = await service.GetCartAsync(1);

        Assert.True(firstAdd.IsSuccess);
        Assert.True(secondAdd.IsSuccess);
        Assert.NotNull(cart);
        Assert.Single(cart!.Items);
        Assert.Equal(3, cart.TotalQuantity);
        Assert.Equal(3, cart.Items[0].Quantity);
        Assert.Equal("597,00", cart.TotalAmount);
    }

    [Fact]
    public async Task CartService_UpdateQuantityAsync_Removes_Item_When_Quantity_Is_Zero()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerCartItems.Add(new CustomerCartItem
        {
            AppUserId = 1,
            ProductId = 1,
            Quantity = 2
        });
        await dbContext.SaveChangesAsync();

        var service = new CartService(dbContext);

        var result = await service.UpdateQuantityAsync(1, "daily-multivitamin", 0);
        var cart = await service.GetCartAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(cart);
        Assert.True(cart!.IsEmpty);
        Assert.Equal(0, result.CartItemCount);
    }

    [Fact]
    public async Task OrderService_PlaceOrderFromCartAsync_Creates_Order_And_Clears_Cart()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerAddresses.Add(new CustomerAddress
        {
            Id = 10,
            AppUserId = 1,
            Title = "Ev",
            RecipientName = "Test User",
            PhoneNumber = "5551112233",
            City = "İstanbul",
            District = "Kadıköy",
            AddressLine = "Adres 1",
            IsDefault = true
        });
        dbContext.CustomerCartItems.Add(new CustomerCartItem
        {
            AppUserId = 1,
            ProductId = 1,
            Quantity = 2
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(dbContext);

        var result = await service.PlaceOrderFromCartAsync(1);
        var order = await dbContext.Orders.Include(x => x.Items).FirstOrDefaultAsync();
        var cartCount = await dbContext.CustomerCartItems.CountAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(order);
        Assert.Equal(2, order!.TotalQuantity);
        Assert.Equal(398m, order.TotalAmount);
        Assert.Single(order.Items);
        Assert.Equal(0, cartCount);
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

    private static CustomerAccountService CreateCustomerAccountService(AppDbContext dbContext)
    {
        return new CustomerAccountService(dbContext, new OrderService(dbContext));
    }
}
