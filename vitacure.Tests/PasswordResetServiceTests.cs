using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Tests;

public class PasswordResetServiceTests
{
    [Fact]
    public async Task CreateResetRequestAsync_Returns_Reset_Link_For_Active_Customer()
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
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "secret1");
        Assert.True(createResult.Succeeded);

        var service = new PasswordResetService(userManager, new AccountAccessService());

        var result = await service.CreateResetRequestAsync(
            "customer@test.local",
            (email, token) => $"/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");

        Assert.Contains("şifre", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/reset-password?", result.ResetUrl, StringComparison.Ordinal);
        Assert.Contains("customer%40test.local", result.ResetUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResetPasswordAsync_Updates_User_Password()
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
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "secret1");
        Assert.True(createResult.Succeeded);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var service = new PasswordResetService(userManager, new AccountAccessService());

        var result = await service.ResetPasswordAsync(new ResetPasswordViewModel
        {
            Email = "customer@test.local",
            Token = token,
            Password = "newpass1",
            ConfirmPassword = "newpass1"
        });

        Assert.True(result.Succeeded);
        Assert.True(await userManager.CheckPasswordAsync(user, "newpass1"));
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
