using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class AgentsController : AdminControllerBase
{
    [HttpGet("/admin/agents")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Agentlar";
        return View();
    }
}
