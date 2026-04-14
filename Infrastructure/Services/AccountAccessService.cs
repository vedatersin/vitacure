using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;

namespace vitacure.Infrastructure.Services;

public class AccountAccessService : IAccountAccessService
{
    public bool CanAccessStorefront(AppUser? user)
    {
        return user is { IsActive: true, EmailConfirmed: true, AccountType: AccountType.Customer };
    }

    public bool CanAccessBackOffice(AppUser? user)
    {
        return user is { IsActive: true, AccountType: AccountType.BackOffice };
    }
}
