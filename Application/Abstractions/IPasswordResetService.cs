using Microsoft.AspNetCore.Identity;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Application.Abstractions;

public interface IPasswordResetService
{
    Task<PasswordResetRequestResultViewModel> CreateResetRequestAsync(string email, Func<string, string, string> buildResetUrl, CancellationToken cancellationToken = default);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model, CancellationToken cancellationToken = default);
}
