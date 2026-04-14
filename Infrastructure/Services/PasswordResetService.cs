using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IAccountAccessService _accountAccessService;
    private readonly UserManager<AppUser> _userManager;

    public PasswordResetService(UserManager<AppUser> userManager, IAccountAccessService accountAccessService)
    {
        _userManager = userManager;
        _accountAccessService = accountAccessService;
    }

    public async Task<PasswordResetRequestResultViewModel> CreateResetRequestAsync(
        string email,
        Func<string, string, string> buildResetUrl,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return new PasswordResetRequestResultViewModel
            {
                Message = "Eğer bu e-posta ile kayıtlı aktif bir müşteri hesabı varsa, şifre sıfırlama bağlantısı oluşturuldu."
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user!);
        return new PasswordResetRequestResultViewModel
        {
            Message = "E-posta servisi henüz aktif değil. Aşağıdaki bağlantı ile şifrenizi şimdi sıfırlayabilirsiniz.",
            ResetUrl = buildResetUrl(user!.Email!, token)
        };
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = model.Email.Trim();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Geçerli bir müşteri hesabı bulunamadı."
            });
        }

        return await _userManager.ResetPasswordAsync(user!, model.Token, model.Password);
    }
}
