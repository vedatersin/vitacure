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
        var notificationCount = await dbContext.AdminNotifications.CountAsync();

        Assert.True(added.IsFavorite);
        Assert.Equal(1, added.FavoriteCount);
        Assert.False(removed.IsFavorite);
        Assert.Equal(0, removed.FavoriteCount);
        Assert.Equal(1, notificationCount);
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
    public async Task UpdateAddressAsync_Updates_Address_And_Default_State()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerAddresses.AddRange(
            new CustomerAddress
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
            },
            new CustomerAddress
            {
                Id = 11,
                AppUserId = 1,
                Title = "Ofis",
                RecipientName = "Test User",
                PhoneNumber = "5554445566",
                City = "İstanbul",
                District = "Şişli",
                AddressLine = "Adres 2",
                IsDefault = false
            });
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var updated = await service.UpdateAddressAsync(1, 11, new AddressFormViewModel
        {
            Title = "Merkez Ofis",
            RecipientName = "Yeni Kisi",
            PhoneNumber = "5559998877",
            City = "Ankara",
            District = "Cankaya",
            AddressLine = "Adres 3",
            PostalCode = "06000",
            IsDefault = true
        });

        var addresses = await dbContext.CustomerAddresses.OrderBy(x => x.Id).ToListAsync();

        Assert.True(updated);
        Assert.False(addresses[0].IsDefault);
        Assert.True(addresses[1].IsDefault);
        Assert.Equal("Merkez Ofis", addresses[1].Title);
        Assert.Equal("Ankara", addresses[1].City);
    }

    [Fact]
    public async Task DeleteAddressAsync_Reassigns_Default_When_Default_Address_Is_Removed()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.CustomerAddresses.AddRange(
            new CustomerAddress
            {
                Id = 10,
                AppUserId = 1,
                Title = "Ev",
                RecipientName = "Test User",
                PhoneNumber = "5551112233",
                City = "İstanbul",
                District = "Kadıköy",
                AddressLine = "Adres 1",
                IsDefault = true,
                CreatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new CustomerAddress
            {
                Id = 11,
                AppUserId = 1,
                Title = "Ofis",
                RecipientName = "Test User",
                PhoneNumber = "5554445566",
                City = "İstanbul",
                District = "Şişli",
                AddressLine = "Adres 2",
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc)
            });
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var deleted = await service.DeleteAddressAsync(1, 10);
        var addresses = await dbContext.CustomerAddresses.OrderBy(x => x.Id).ToListAsync();

        Assert.True(deleted);
        Assert.Single(addresses);
        Assert.True(addresses[0].IsDefault);
        Assert.Equal(11, addresses[0].Id);
    }

    [Fact]
    public async Task UpdateProfileAsync_Updates_Basic_Profile_Fields()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        await dbContext.SaveChangesAsync();

        var service = CreateCustomerAccountService(dbContext);

        var updated = await service.UpdateProfileAsync(1, new ProfileFormViewModel
        {
            FullName = "Yeni Kullanici",
            Email = "yeni@test.local",
            PhoneNumber = "5557778899"
        });

        var user = await dbContext.Users.FirstAsync(x => x.Id == 1);

        Assert.True(updated);
        Assert.Equal("Yeni Kullanici", user.FullName);
        Assert.Equal("yeni@test.local", user.Email);
        Assert.Equal("yeni@test.local", user.UserName);
        Assert.Equal("5557778899", user.PhoneNumber);
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

        var service = new CartService(dbContext, CreateAdminNotificationService(dbContext));

        var firstAdd = await service.AddItemAsync(1, "daily-multivitamin");
        var secondAdd = await service.AddItemAsync(1, "daily-multivitamin", 2);
        var cart = await service.GetCartAsync(1);
        var notificationCount = await dbContext.AdminNotifications.CountAsync();

        Assert.True(firstAdd.IsSuccess);
        Assert.True(secondAdd.IsSuccess);
        Assert.NotNull(cart);
        Assert.Single(cart!.Items);
        Assert.Equal(3, cart.TotalQuantity);
        Assert.Equal(3, cart.Items[0].Quantity);
        Assert.Equal("597,00", cart.TotalAmount);
        Assert.Equal(2, notificationCount);
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

        var service = new CartService(dbContext, CreateAdminNotificationService(dbContext));

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

        var service = new OrderService(dbContext, CreateAdminNotificationService(dbContext));

        var result = await service.PlaceOrderFromCartAsync(1);
        var order = await dbContext.Orders.Include(x => x.Items).FirstOrDefaultAsync();
        var cartCount = await dbContext.CustomerCartItems.CountAsync();
        var notificationCount = await dbContext.AdminNotifications.CountAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(order);
        Assert.Equal(2, order!.TotalQuantity);
        Assert.Equal(398m, order.TotalAmount);
        Assert.Single(order.Items);
        Assert.Equal(0, cartCount);
        Assert.Equal(1, notificationCount);
    }

    [Fact]
    public async Task CartService_AddItemAsync_Keeps_Product_Variants_As_Separate_Lines()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.ProductVariants.AddRange(
            new ProductVariant
            {
                Id = 11,
                ProductId = 1,
                GroupName = "Boyut",
                OptionName = "30 Tablet",
                Price = 199m,
                Stock = 10,
                SortOrder = 0,
                IsActive = true
            },
            new ProductVariant
            {
                Id = 12,
                ProductId = 1,
                GroupName = "Boyut",
                OptionName = "60 Tablet",
                Price = 299m,
                Stock = 10,
                SortOrder = 1,
                IsActive = true
            });
        await dbContext.SaveChangesAsync();

        var service = new CartService(dbContext, CreateAdminNotificationService(dbContext));

        await service.AddItemAsync(1, "daily-multivitamin", 1, 11);
        await service.AddItemAsync(1, "daily-multivitamin", 2, 12);
        var cart = await service.GetCartAsync(1);

        Assert.NotNull(cart);
        Assert.Equal(2, cart!.Items.Count);
        Assert.Contains(cart.Items, x => x.VariantId == 11 && x.Quantity == 1 && x.LineTotal == "199,00");
        Assert.Contains(cart.Items, x => x.VariantId == 12 && x.Quantity == 2 && x.LineTotal == "598,00");
        Assert.Equal("797,00", cart.TotalAmount);
    }

    [Fact]
    public async Task OrderService_PlaceOrderFromCartAsync_Persists_Variant_Label()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.ProductVariants.Add(new ProductVariant
        {
            Id = 11,
            ProductId = 1,
            GroupName = "Boyut",
            OptionName = "60 Tablet",
            Price = 299m,
            Stock = 10,
            SortOrder = 0,
            IsActive = true
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
        dbContext.CustomerCartItems.Add(new CustomerCartItem
        {
            AppUserId = 1,
            ProductId = 1,
            ProductVariantId = 11,
            Quantity = 2
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(dbContext, CreateAdminNotificationService(dbContext));

        var result = await service.PlaceOrderFromCartAsync(1);
        var order = await dbContext.Orders.Include(x => x.Items).FirstAsync();
        var orderItem = order.Items.First();

        Assert.True(result.IsSuccess);
        Assert.Equal(598m, order.TotalAmount);
        Assert.Equal("Boyut: 60 Tablet", orderItem.VariantLabel);
        Assert.Equal(11, orderItem.ProductVariantId);
    }

    [Fact]
    public async Task OrderService_PlaceOrderFromCartAsync_Decrements_Stocks_For_Base_Product()
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
            Quantity = 4
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(dbContext, CreateAdminNotificationService(dbContext));

        var result = await service.PlaceOrderFromCartAsync(1);
        var product = await dbContext.Products.FirstAsync(x => x.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(11, product.Stock);
    }

    [Fact]
    public async Task OrderService_PlaceOrderFromCartAsync_Decrements_Stocks_For_Variant_Product()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.ProductVariants.Add(new ProductVariant
        {
            Id = 11,
            ProductId = 1,
            GroupName = "Boyut",
            OptionName = "60 Tablet",
            Price = 299m,
            Stock = 10,
            SortOrder = 0,
            IsActive = true
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
        dbContext.CustomerCartItems.Add(new CustomerCartItem
        {
            AppUserId = 1,
            ProductId = 1,
            ProductVariantId = 11,
            Quantity = 3
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(dbContext, CreateAdminNotificationService(dbContext));

        var result = await service.PlaceOrderFromCartAsync(1);
        var product = await dbContext.Products.FirstAsync(x => x.Id == 1);
        var variant = await dbContext.ProductVariants.FirstAsync(x => x.Id == 11);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, product.Stock);
        Assert.Equal(7, variant.Stock);
    }

    [Fact]
    public async Task OrderService_PlaceOrderFromCartAsync_Returns_Error_When_Variant_Stock_Is_Not_Enough()
    {
        await using var dbContext = CreateDbContext();
        SeedUserAndProduct(dbContext);
        dbContext.ProductVariants.Add(new ProductVariant
        {
            Id = 11,
            ProductId = 1,
            GroupName = "Boyut",
            OptionName = "60 Tablet",
            Price = 299m,
            Stock = 1,
            SortOrder = 0,
            IsActive = true
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
        dbContext.CustomerCartItems.Add(new CustomerCartItem
        {
            AppUserId = 1,
            ProductId = 1,
            ProductVariantId = 11,
            Quantity = 2
        });
        await dbContext.SaveChangesAsync();

        var service = new OrderService(dbContext, CreateAdminNotificationService(dbContext));

        var result = await service.PlaceOrderFromCartAsync(1);
        var orderCount = await dbContext.Orders.CountAsync();
        var variant = await dbContext.ProductVariants.FirstAsync(x => x.Id == 11);

        Assert.False(result.IsSuccess);
        Assert.Contains("yeterli stok yok", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, orderCount);
        Assert.Equal(1, variant.Stock);
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
        return new CustomerAccountService(
            dbContext,
            new OrderService(dbContext, CreateAdminNotificationService(dbContext)),
            CreateAdminNotificationService(dbContext));
    }

    private static AdminNotificationService CreateAdminNotificationService(AppDbContext dbContext)
    {
        return new AdminNotificationService(dbContext);
    }
}
