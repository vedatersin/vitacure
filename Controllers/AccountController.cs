using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ICustomerAccountService _customerAccountService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IAccountAccessService accountAccessService,
        ICustomerAccountService customerAccountService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountAccessService = accountAccessService;
        _customerAccountService = customerAccountService;
    }

    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            ModelState.AddModelError(string.Empty, "Geçerli bir müşteri hesabı bulunamadı.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost("/register")]
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
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
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

        return View(model);
    }

    [Authorize]
    [HttpPost("/account/favorites/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteToggleRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!_accountAccessService.CanAccessStorefront(user))
        {
            return Unauthorized();
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
            return View("Index", invalidModel);
        }

        await _customerAccountService.AddAddressAsync(user!.Id, model, cancellationToken);
        return RedirectToAction(nameof(Index), new { section = "addresses" });
    }
}

public class FavoriteToggleRequest
{
    public string ProductSlug { get; set; } = string.Empty;
}
