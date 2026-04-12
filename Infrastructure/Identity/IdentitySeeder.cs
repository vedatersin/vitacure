using Microsoft.AspNetCore.Identity;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;

namespace vitacure.Infrastructure.Identity;

public class IdentitySeeder
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public IdentitySeeder(
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        await EnsureRoleAsync("Customer", isBackOfficeRole: false);
        await EnsureRoleAsync("Admin", isBackOfficeRole: true);
        await EnsureRoleAsync("Editor", isBackOfficeRole: true);

        var adminEmail = _configuration["SeedSettings:AdminEmail"];
        var adminPassword = _configuration["SeedSettings:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null)
        {
            return;
        }

        adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Vitacure Admin",
            EmailConfirmed = true,
            IsActive = true,
            AccountType = AccountType.BackOffice,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Default admin user could not be created: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Default admin role could not be assigned: {errors}");
        }
    }

    private async Task EnsureRoleAsync(string roleName, bool isBackOfficeRole)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is not null)
        {
            return;
        }

        var result = await _roleManager.CreateAsync(new AppRole
        {
            Name = roleName,
            IsBackOfficeRole = isBackOfficeRole
        });

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Role '{roleName}' could not be created: {errors}");
        }
    }
}
