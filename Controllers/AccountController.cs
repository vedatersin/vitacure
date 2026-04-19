using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Models.ViewModels.Account;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAccountAccessService _accountAccessService;
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly ICustomerAccountService _customerAccountService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly IGuestSessionService _guestSessionService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IAccountAccessService accountAccessService,
        IAdminNotificationService adminNotificationService,
        ICustomerAccountService customerAccountService,
        IEmailConfirmationService emailConfirmationService,
        IGuestSessionService guestSessionService,
        IPasswordResetService passwordResetService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountAccessService = accountAccessService;
        _adminNotificationService = adminNotificationService;
        _customerAccountService = customerAccountService;
        _emailConfirmationService = emailConfirmationService;
        _guestSessionService = guestSessionService;
        _passwordResetService = passwordResetService;
    }

    [HttpGet("/login")]
    [EnableRateLimiting("auth")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/login")]
    [EnableRateLimiting("auth")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            ModelState.AddModelError(string.Empty, user is { EmailConfirmed: false, AccountType: AccountType.Customer, IsActive: true }
                ? "E-posta adresinizi doğrulamadan giriş yapamazsınız."
                : "Geçerli bir müşteri hesabı bulunamadı.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        try
        {
            await _guestSessionService.MergeIntoCustomerAccountAsync(user!.Id, cancellationToken);
        }
        catch
        {
            TempData["AuthMessage"] = "Gecici sepet ve favoriler hesaba aktarilamadi. Gecici oturum verileri korunuyor.";
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/register")]
    [EnableRateLimiting("auth")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost("/register")]
    [EnableRateLimiting("auth")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            AccountType = AccountType.Customer,
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        var confirmation = await _emailConfirmationService.BuildConfirmationAsync(
            user,
            (email, token) => Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Account",
                values: new { email, token },
                protocol: Request.Scheme) ?? string.Empty);

        await _adminNotificationService.CreateAsync(new AdminNotificationCreateRequest
        {
            Title = "Yeni uye kaydi olustu",
            Summary = $"{user.FullName} hesabi olusturuldu ve e-posta dogrulamasi bekleniyor.",
            Body = $"{user.FullName} kullanicisi storefront hesabi acti. Dogrulama baglantisi hazirlandi ve auth akislarindan takip ediliyor.",
            Actor = user.FullName,
            Source = "Auth",
            CategoryKey = "members",
            TargetLabel = "Kullanicilara git",
            TargetUrl = "/admin/users",
            OccurredAt = user.CreatedAt
        });

        return View("RegisterConfirmation", new RegisterConfirmationViewModel
        {
            Email = user.Email ?? string.Empty,
            Message = confirmation.Message,
            ConfirmationUrl = confirmation.ConfirmationUrl
        });
    }

    [HttpGet("/confirm-email")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(Register));
        }

        var result = await _emailConfirmationService.ConfirmEmailAsync(email, token);
        if (!result.Succeeded)
        {
            TempData["AuthMessage"] = "E-posta doğrulama bağlantısı geçersiz veya süresi dolmuş olabilir.";
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is not null)
        {
            await _adminNotificationService.CreateAsync(new AdminNotificationCreateRequest
            {
                Title = "E-posta dogrulamasi tamamlandi",
                Summary = $"{user.FullName} hesabinin e-posta dogrulamasi tamamlandi.",
                Body = $"{user.FullName} kullanicisi hesabini dogrulayarak storefront girisi icin hazir hale geldi.",
                Actor = user.FullName,
                Source = "Auth",
                CategoryKey = "auth",
                TargetLabel = "Kullanicilara git",
                TargetUrl = "/admin/users"
            });
        }

        return View("ConfirmEmailSuccess");
    }

    [HttpGet("/forgot-password")]
    [EnableRateLimiting("auth")]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost("/forgot-password")]
    [EnableRateLimiting("auth")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _passwordResetService.CreateResetRequestAsync(
            model.Email,
            (email, token) => Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values: new { email, token },
                protocol: Request.Scheme) ?? string.Empty,
            cancellationToken);

        model.Message = result.Message;
        model.ResetUrl = result.ResetUrl;

        if (!string.IsNullOrWhiteSpace(result.ResetUrl))
        {
            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user is not null)
            {
                await _adminNotificationService.CreateAsync(new AdminNotificationCreateRequest
                {
                    Title = "Parola sifirlama talebi olustu",
                    Summary = $"{user.FullName} icin sifre sifirlama baglantisi uretildi.",
                    Body = $"{user.FullName} kullanicisi parola sifirlama talebi baslatti. Baglanti e-posta servisi aktif olana kadar manuel olarak sunuluyor.",
                    Actor = user.FullName,
                    Source = "Auth",
                    CategoryKey = "auth",
                    TargetLabel = "Kullanicilara git",
                    TargetUrl = "/admin/users"
                }, cancellationToken);
            }
        }

        return View(model);
    }

    [HttpGet("/reset-password")]
    [EnableRateLimiting("auth")]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        });
    }

    [HttpPost("/reset-password")]
    [EnableRateLimiting("auth")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _passwordResetService.ResetPasswordAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        return View("ResetPasswordSuccess");
    }

    [Authorize]
    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet("/account")]
    public async Task<IActionResult> Index(int? editAddressId, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return RedirectToAction("Login");
        }

        var model = await _customerAccountService.GetDashboardAsync(user!.Id, cancellationToken);
        if (model is null)
        {
            return RedirectToAction("Login");
        }

        if (editAddressId.HasValue)
        {
            var address = model.Addresses.FirstOrDefault(x => x.Id == editAddressId.Value);
            if (address is not null)
            {
                model.EditingAddressId = address.Id;
                model.EditAddress = new AddressFormViewModel
                {
                    Id = address.Id,
                    Title = address.Title,
                    RecipientName = address.RecipientName,
                    PhoneNumber = address.PhoneNumber,
                    City = address.City,
                    District = address.District,
                    AddressLine = address.AddressLine,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault
                };
            }
        }

        ApplySectionState(model);

        return View(model);
    }

    [HttpPost("/account/favorites/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteToggleRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return Json(_guestSessionService.ToggleFavorite(request.ProductSlug));
        }

        var result = await _customerAccountService.ToggleFavoriteAsync(user!.Id, request.ProductSlug, cancellationToken);
        return Json(result);
    }

    [Authorize]
    [HttpPost("/account/addresses/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress(AddressFormViewModel model, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _customerAccountService.GetDashboardAsync(user!.Id, cancellationToken);
            if (invalidModel is null)
            {
                return RedirectToAction("Login");
            }

            invalidModel.NewAddress = model;
            invalidModel.EditingAddressId = null;
            ApplySectionState(invalidModel, "addresses");
            return View("Index", invalidModel);
        }

        await _customerAccountService.AddAddressAsync(user!.Id, model, cancellationToken);
        return RedirectToAction(nameof(Index), new { section = "addresses" });
    }

    [Authorize]
    [HttpPost("/account/addresses/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAddress(AddressFormViewModel model, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _customerAccountService.GetDashboardAsync(user!.Id, cancellationToken);
            if (invalidModel is null)
            {
                return RedirectToAction("Login");
            }

            invalidModel.EditAddress = model;
            invalidModel.EditingAddressId = model.Id;
            ApplySectionState(invalidModel, "addresses");
            return View("Index", invalidModel);
        }

        var updated = await _customerAccountService.UpdateAddressAsync(user!.Id, model.Id, model, cancellationToken);
        if (!updated)
        {
            TempData["AccountMessage"] = "Adres guncellenemedi.";
        }

        return RedirectToAction(nameof(Index), new { section = "addresses" });
    }

    [Authorize]
    [HttpPost("/account/addresses/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int addressId, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return RedirectToAction("Login");
        }

        var deleted = await _customerAccountService.DeleteAddressAsync(user!.Id, addressId, cancellationToken);
        if (!deleted)
        {
            TempData["AccountMessage"] = "Adres silinemedi.";
        }

        return RedirectToAction(nameof(Index), new { section = "addresses" });
    }

    [Authorize]
    [HttpPost("/account/profile/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileFormViewModel model, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _customerAccountService.GetDashboardAsync(user!.Id, cancellationToken);
            if (invalidModel is null)
            {
                return RedirectToAction("Login");
            }

            invalidModel.Profile = model;
            ApplySectionState(invalidModel, "profile");
            return View("Index", invalidModel);
        }

        var updated = await _customerAccountService.UpdateProfileAsync(user!.Id, model, cancellationToken);
        if (!updated)
        {
            ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi baska bir hesapta kullaniliyor olabilir.");
            var invalidModel = await _customerAccountService.GetDashboardAsync(user!.Id, cancellationToken);
            if (invalidModel is null)
            {
                return RedirectToAction("Login");
            }

            invalidModel.Profile = model;
            ApplySectionState(invalidModel, "profile");
            return View("Index", invalidModel);
        }

        TempData["AccountMessage"] = "Profil bilgileri guncellendi.";
        return RedirectToAction(nameof(Index), new { section = "profile" });
    }

    private void ApplySectionState(AccountDashboardViewModel model, string? fallbackSection = null)
    {
        ViewData["AccountSection"] = Request.Query["section"].FirstOrDefault() ?? fallbackSection ?? "profile";
    }
}

public class FavoriteToggleRequest
{
    public string ProductSlug { get; set; } = string.Empty;
}
