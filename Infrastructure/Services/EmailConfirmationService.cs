using Microsoft.AspNetCore.Identity;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Infrastructure.Services;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly UserManager<AppUser> _userManager;

    public EmailConfirmationService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<EmailConfirmationRequestResultViewModel> BuildConfirmationAsync(
        AppUser user,
        Func<string, string, string> buildConfirmationUrl)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        return new EmailConfirmationRequestResultViewModel
        {
            Message = "E-posta servisi henüz aktif değil. Hesabınızı doğrulamak için aşağıdaki bağlantıyı kullanın.",
            ConfirmationUrl = buildConfirmationUrl(user.Email!, token)
        };
    }

    public async Task<IdentityResult> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Geçerli bir kullanıcı bulunamadı."
            });
        }

        return await _userManager.ConfirmEmailAsync(user, token);
    }
}
