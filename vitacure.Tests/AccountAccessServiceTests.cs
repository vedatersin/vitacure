using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class AccountAccessServiceTests
{
    [Fact]
    public void CanAccessStorefront_Returns_True_Only_For_Active_Customer()
    {
        var service = new AccountAccessService();

        Assert.True(service.CanAccessStorefront(new AppUser { IsActive = true, EmailConfirmed = true, AccountType = AccountType.Customer }));
        Assert.False(service.CanAccessStorefront(new AppUser { IsActive = true, EmailConfirmed = false, AccountType = AccountType.Customer }));
        Assert.False(service.CanAccessStorefront(new AppUser { IsActive = true, AccountType = AccountType.BackOffice }));
        Assert.False(service.CanAccessStorefront(new AppUser { IsActive = false, AccountType = AccountType.Customer }));
        Assert.False(service.CanAccessStorefront(null));
    }

    [Fact]
    public void CanAccessBackOffice_Returns_True_Only_For_Active_BackOffice_User()
    {
        var service = new AccountAccessService();

        Assert.True(service.CanAccessBackOffice(new AppUser { IsActive = true, AccountType = AccountType.BackOffice }));
        Assert.False(service.CanAccessBackOffice(new AppUser { IsActive = true, AccountType = AccountType.Customer }));
        Assert.False(service.CanAccessBackOffice(new AppUser { IsActive = false, AccountType = AccountType.BackOffice }));
        Assert.False(service.CanAccessBackOffice(null));
    }
}
