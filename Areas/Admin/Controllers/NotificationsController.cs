using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class NotificationsController : Controller
{
    private readonly IAdminNotificationService _adminNotificationService;

    public NotificationsController(IAdminNotificationService adminNotificationService)
    {
        _adminNotificationService = adminNotificationService;
    }

    [HttpGet("/admin/notifications")]
    public async Task<IActionResult> Index([FromQuery] string? category, [FromQuery] int? notificationId, CancellationToken cancellationToken)
    {
        var model = await _adminNotificationService.GetModuleAsync(category, notificationId, cancellationToken);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Notifications/_NotificationContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/notifications/summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var summary = await _adminNotificationService.GetSummaryAsync(cancellationToken: cancellationToken);
        return Json(summary);
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
