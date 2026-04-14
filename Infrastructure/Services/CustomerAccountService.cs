using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels;
using vitacure.Models.ViewModels.Account;

namespace vitacure.Infrastructure.Services;

public class CustomerAccountService : ICustomerAccountService
{
    private readonly AppDbContext _dbContext;
    private readonly IOrderService _orderService;

    public CustomerAccountService(AppDbContext dbContext, IOrderService orderService)
    {
        _dbContext = dbContext;
        _orderService = orderService;
    }

    public async Task<AccountDashboardViewModel?> GetDashboardAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Addresses.OrderByDescending(address => address.IsDefault).ThenBy(address => address.Title))
            .Include(x => x.Favorites)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x!.Category)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var favoriteProducts = user.Favorites
            .Where(x => x.Product is not null && x.Product.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => BuildProductCard(x.Product!))
            .ToList();
        var orders = await _orderService.GetOrderHistoryAsync(userId, cancellationToken);

        return new AccountDashboardViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            FavoriteCount = favoriteProducts.Count,
            AddressCount = user.Addresses.Count,
            OrderCount = orders.Count,
            FavoriteProducts = favoriteProducts,
            Orders = orders,
            Profile = new ProfileFormViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            },
            Addresses = user.Addresses.Select(address => new AccountAddressViewModel
            {
                Id = address.Id,
                Title = address.Title,
                RecipientName = address.RecipientName,
                PhoneNumber = address.PhoneNumber,
                City = address.City,
                District = address.District,
                AddressLine = address.AddressLine,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<string>> GetFavoriteProductSlugsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomerFavorites
            .AsNoTracking()
            .Where(x => x.AppUserId == userId)
            .Select(x => x.Product!.Slug)
            .ToListAsync(cancellationToken);
    }

    public async Task<FavoriteToggleResultViewModel> ToggleFavoriteAsync(int userId, string productSlug, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Slug == productSlug && x.IsActive, cancellationToken);

        if (product is null)
        {
            return new FavoriteToggleResultViewModel();
        }

        var favorite = await _dbContext.CustomerFavorites
            .FirstOrDefaultAsync(x => x.AppUserId == userId && x.ProductId == product.Id, cancellationToken);

        var isFavorite = favorite is null;
        if (favorite is null)
        {
            _dbContext.CustomerFavorites.Add(new CustomerFavorite
            {
                AppUserId = userId,
                ProductId = product.Id
            });
        }
        else
        {
            _dbContext.CustomerFavorites.Remove(favorite);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var favoriteCount = await _dbContext.CustomerFavorites.CountAsync(x => x.AppUserId == userId, cancellationToken);
        return new FavoriteToggleResultViewModel
        {
            IsFavorite = isFavorite,
            FavoriteCount = favoriteCount
        };
    }

    public async Task<bool> AddAddressAsync(int userId, AddressFormViewModel model, CancellationToken cancellationToken = default)
    {
        var userExists = await _dbContext.Users.AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            return false;
        }

        if (model.IsDefault)
        {
            var currentDefaults = await _dbContext.CustomerAddresses
                .Where(x => x.AppUserId == userId && x.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var address in currentDefaults)
            {
                address.IsDefault = false;
            }
        }

        _dbContext.CustomerAddresses.Add(new CustomerAddress
        {
            AppUserId = userId,
            Title = model.Title.Trim(),
            RecipientName = model.RecipientName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            City = model.City.Trim(),
            District = model.District.Trim(),
            AddressLine = model.AddressLine.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(model.PostalCode) ? null : model.PostalCode.Trim(),
            IsDefault = model.IsDefault
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAddressAsync(int userId, int addressId, AddressFormViewModel model, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.AppUserId == userId, cancellationToken);

        if (address is null)
        {
            return false;
        }

        if (model.IsDefault)
        {
            var currentDefaults = await _dbContext.CustomerAddresses
                .Where(x => x.AppUserId == userId && x.Id != addressId && x.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existingAddress in currentDefaults)
            {
                existingAddress.IsDefault = false;
            }
        }

        address.Title = model.Title.Trim();
        address.RecipientName = model.RecipientName.Trim();
        address.PhoneNumber = model.PhoneNumber.Trim();
        address.City = model.City.Trim();
        address.District = model.District.Trim();
        address.AddressLine = model.AddressLine.Trim();
        address.PostalCode = string.IsNullOrWhiteSpace(model.PostalCode) ? null : model.PostalCode.Trim();
        address.IsDefault = model.IsDefault;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.AppUserId == userId, cancellationToken);

        if (address is null)
        {
            return false;
        }

        var deletedWasDefault = address.IsDefault;
        _dbContext.CustomerAddresses.Remove(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!deletedWasDefault)
        {
            return true;
        }

        var nextAddress = await _dbContext.CustomerAddresses
            .Where(x => x.AppUserId == userId)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextAddress is null)
        {
            return true;
        }

        nextAddress.IsDefault = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateProfileAsync(int userId, ProfileFormViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var normalizedEmail = model.Email.Trim();
        var emailInUse = await _dbContext.Users.AnyAsync(
            x => x.Id != userId && x.Email != null && x.Email.ToLower() == normalizedEmail.ToLower(),
            cancellationToken);

        if (emailInUse)
        {
            return false;
        }

        user.FullName = model.FullName.Trim();
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> GetOrderCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders.CountAsync(x => x.AppUserId == userId, cancellationToken);
    }

    private static ProductCardViewModel BuildProductCard(Product product)
    {
        return new ProductCardViewModel
        {
            Id = product.Slug,
            Name = product.Name,
            SizeLabel = BuildProductSizeLabel(product.Name),
            ImageUrl = product.ImageUrl,
            Price = product.Price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ","),
            OldPrice = product.OldPrice?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",") ?? string.Empty,
            Rating = product.Rating.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
            RatingWidth = $"{Math.Round(product.Rating / 5m * 100m, MidpointRounding.AwayFromZero)}%",
            Description = product.Description ?? string.Empty,
            Href = $"/urun/{product.Slug}",
            CartProductSlug = product.Slug
        };
    }

    private static string BuildProductSizeLabel(string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return "60 kapsul";
        }

        return productName.Trim() switch
        {
            "Daily Multivitamin" => "120 kapsul",
            "Omega 3" => "60 softgel",
            "Vitamin D3" => "30 ml damla",
            "Magnezyum" => "60 kapsul",
            "C Vitamini Complex" => "20 efervesan tablet",
            "Kolajen Peptit" => "300 gr toz",
            "B12 Vitamini" => "30 ml sprey",
            "Çinko Pikolinat" => "90 kapsul",
            "Probiyotik 10B" => "30 kapsul",
            "Demir + C Vitamini" => "30 kapsul",
            "Kalsiyum Kompleks" => "60 tablet",
            "B12 Vitamini Sprey" => "20 ml sprey",
            "Vitamin D3 - 3 Al 2 Öde" => "3 x 30 ml",
            "Omega 3 Aile Paketi" => "2 x 60 softgel",
            "C Vitamini Seti" => "3 x 20 tablet",
            "Magnezyum Enerji Kofre" => "2 x 60 kapsul",
            "Daily Multivitamin Büyük Boy" => "180 kapsul",
            "Kolajen ve C Vitamini" => "14 saşe",
            "Çinko Kompleks" => "60 tablet",
            "Probiyotik Bakteri" => "20 kapsul",
            "Demir Takviyesi" => "30 kapsul",
            _ => "60 kapsul"
        };
    }
}
