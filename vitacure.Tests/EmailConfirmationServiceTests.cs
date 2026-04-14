using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class EmailConfirmationServiceTests
{
    [Fact]
    public async Task BuildConfirmationAsync_Returns_Confirmation_Link()
    {
        using var services = CreateServices();
        await using var dbContext = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            UserName = "customer@test.local",
            Email = "customer@test.local",
            FullName = "Customer User",
            AccountType = AccountType.Customer,
            IsActive = true,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, "secret1");
        Assert.True(createResult.Succeeded);

        var service = new EmailConfirmationService(userManager);

        var result = await service.BuildConfirmationAsync(
            user,
            (email, token) => $"/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");

        Assert.Contains("/confirm-email?", result.ConfirmationUrl, StringComparison.Ordinal);
        Assert.Contains("customer%40test.local", result.ConfirmationUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConfirmEmailAsync_Marks_User_As_Confirmed()
    {
        using var services = CreateServices();
        await using var dbContext = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            UserName = "customer@test.local",
            Email = "customer@test.local",
            FullName = "Customer User",
            AccountType = AccountType.Customer,
            IsActive = true,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, "secret1");
        Assert.True(createResult.Succeeded);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var service = new EmailConfirmationService(userManager);

        var result = await service.ConfirmEmailAsync(user.Email!, token);
        var refreshedUser = await userManager.FindByEmailAsync(user.Email!);

        Assert.True(result.Succeeded);
        Assert.True(refreshedUser!.EmailConfirmed);
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }
}
