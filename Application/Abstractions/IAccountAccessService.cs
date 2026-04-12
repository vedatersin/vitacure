using vitacure.Domain.Entities;

namespace vitacure.Application.Abstractions;

public interface IAccountAccessService
{
    bool CanAccessStorefront(AppUser? user);
    bool CanAccessBackOffice(AppUser? user);
}
