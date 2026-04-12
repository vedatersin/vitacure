using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class AdminUserServiceTests
{
    [Fact]
    public async Task GetUsersAsync_Returns_Users_With_Role_Summary_And_Counts()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Users.AddRange(
            new AppUser
            {
                Id = 1,
                UserName = "customer@test.local",
                Email = "customer@test.local",
                FullName = "Customer User",
                AccountType = AccountType.Customer,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new AppUser
            {
                Id = 2,
                UserName = "admin@test.local",
                Email = "admin@test.local",
                FullName = "Admin User",
                AccountType = AccountType.BackOffice,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            });

        dbContext.Roles.Add(new AppRole
        {
            Id = 10,
            Name = "Admin",
            NormalizedName = "ADMIN",
            IsBackOfficeRole = true
        });

        dbContext.UserRoles.Add(new IdentityUserRole<int>
        {
            UserId = 2,
            RoleId = 10
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminUserService(dbContext);

        var result = await service.GetUsersAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.CustomerCount);
        Assert.Equal(1, result.BackOfficeCount);
        Assert.Equal("Admin", result.Users.First(x => x.Id == 2).RoleSummary);
        Assert.Equal("Müşteri", result.Users.First(x => x.Id == 1).AccountTypeLabel);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
