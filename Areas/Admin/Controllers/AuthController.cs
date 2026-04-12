using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Models.ViewModels.Auth;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IAccountAccessService _accountAccessService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IAccountAccessService accountAccessService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountAccessService = accountAccessService;
    }

    [HttpGet("/admin/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/admin/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (!_accountAccessService.CanAccessBackOffice(user))
        {
            ModelState.AddModelError(string.Empty, "Geçerli bir yönetim hesabı bulunamadı.");
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

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost("/admin/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
