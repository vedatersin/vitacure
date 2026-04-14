using Microsoft.AspNetCore.Identity;
using vitacure.Domain.Entities;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Application.Abstractions;

public interface IEmailConfirmationService
{
    Task<EmailConfirmationRequestResultViewModel> BuildConfirmationAsync(AppUser user, Func<string, string, string> buildConfirmationUrl);
    Task<IdentityResult> ConfirmEmailAsync(string email, string token);
}
