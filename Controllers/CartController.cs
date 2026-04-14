using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Models.ViewModels.Cart;

namespace vitacure.Controllers;

public class CartController : Controller
{
    private readonly IAccountAccessService _accountAccessService;
    private readonly ICartService _cartService;
    private readonly IGuestSessionService _guestSessionService;
    private readonly IOrderService _orderService;
    private readonly UserManager<AppUser> _userManager;

    public CartController(
        UserManager<AppUser> userManager,
        IAccountAccessService accountAccessService,
        ICartService cartService,
        IGuestSessionService guestSessionService,
        IOrderService orderService)
    {
        _userManager = userManager;
        _accountAccessService = accountAccessService;
        _cartService = cartService;
        _guestSessionService = guestSessionService;
        _orderService = orderService;
    }

    [HttpGet("/cart")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        var model = CanAccessCart(user)
            ? await _cartService.GetCartAsync(user!.Id, cancellationToken)
            : await _guestSessionService.GetCartAsync(cancellationToken);

        ViewData["Title"] = "Sepetim";
        ViewData["MetaDescription"] = "Vitacure sepetinizdeki urunleri goruntuleyin ve siparisinizi hazirlayin.";
        ViewData["CanonicalPath"] = "/cart";
        ViewData["RequiresLoginForCheckout"] = !CanAccessCart(user);

        return View(model ?? new CartViewModel());
    }

    [HttpPost("/cart/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!CanAccessCart(user))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/cart" });
        }

        var result = await _orderService.PlaceOrderFromCartAsync(user!.Id, cancellationToken);
        TempData["CheckoutMessage"] = result.Message;
        TempData["CheckoutMessageType"] = result.IsSuccess ? "success" : "error";

        if (result.IsSuccess)
        {
            return RedirectToAction("Index", "Account", new { section = "orders" });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = CanAccessCart(user)
            ? await _cartService.AddItemAsync(user!.Id, request.ProductSlug, request.Quantity, cancellationToken)
            : await _guestSessionService.AddCartItemAsync(request.ProductSlug, request.Quantity, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Json(result);
    }

    [HttpPost("/cart/items/{productSlug}/quantity")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(string productSlug, [FromForm] int quantity, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (CanAccessCart(user))
        {
            await _cartService.UpdateQuantityAsync(user!.Id, productSlug, quantity, cancellationToken);
        }
        else
        {
            await _guestSessionService.UpdateCartQuantityAsync(productSlug, quantity, cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/items/{productSlug}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(string productSlug, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (CanAccessCart(user))
        {
            await _cartService.RemoveItemAsync(user!.Id, productSlug, cancellationToken);
        }
        else
        {
            await _guestSessionService.RemoveCartItemAsync(productSlug, cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CanAccessCart(AppUser? user)
    {
        return user is not null
            && _accountAccessService.CanAccessStorefront(user)
            && user.AccountType == AccountType.Customer;
    }
}
