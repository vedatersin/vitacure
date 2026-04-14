using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

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
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, [FromQuery] string? volume, CancellationToken cancellationToken)
    {
        var model = await _adminOrderService.GetOrdersAsync(cancellationToken);
        model = ApplyFilters(model, q, status, volume);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Orders/_ListContent.cshtml", model);
        }

        return View(model);
    }

    private static AdminOrderListViewModel ApplyFilters(AdminOrderListViewModel model, string? q, string? status, string? volume)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();
        var normalizedVolume = string.IsNullOrWhiteSpace(volume) ? "all" : volume.Trim().ToLowerInvariant();

        IEnumerable<AdminOrderListItemViewModel> query = model.Orders;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.OrderNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.CustomerEmail.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = normalizedStatus switch
        {
            "pending" => query.Where(item => item.Status is "Beklemede" or "Hazırlanıyor" or "Hazirlanıyor"),
            "completed" => query.Where(item => item.Status == "Tamamlandı" || item.Status == "Tamamlandi"),
            "cancelled" => query.Where(item => item.Status == "İptal" || item.Status == "Iptal"),
            _ => query
        };

        query = normalizedVolume switch
        {
            "single" => query.Where(item => item.TotalQuantity == 1),
            "multi" => query.Where(item => item.TotalQuantity > 1),
            _ => query
        };

        var items = query.ToList();

        return new AdminOrderListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            VolumeFilter = normalizedVolume,
            TotalCount = items.Count,
            PendingCount = items.Count(item => item.Status is "Beklemede" or "Hazırlanıyor" or "Hazirlanıyor"),
            CompletedCount = items.Count(item => item.Status == "Tamamlandı" || item.Status == "Tamamlandi"),
            TotalRevenue = items.Sum(item => item.TotalAmount),
            Orders = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
