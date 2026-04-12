using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class OrdersController : Controller
{
    private readonly IAdminOrderService _adminOrderService;

    public OrdersController(IAdminOrderService adminOrderService)
    {
        _adminOrderService = adminOrderService;
    }

    [HttpGet("/admin/orders")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminOrderService.GetOrdersAsync(cancellationToken);
        return View(model);
    }
}
