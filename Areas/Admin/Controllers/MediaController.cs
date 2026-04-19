using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class MediaController : AdminControllerBase
{
    private readonly IAdminMediaLibraryService _adminMediaLibraryService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IAdminMediaLibraryService adminMediaLibraryService, ILogger<MediaController> logger)
    {
        _adminMediaLibraryService = adminMediaLibraryService;
        _logger = logger;
    }

    [HttpGet("/admin/media")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminMediaLibraryService.GetLibraryAsync(cancellationToken);
        return View(model);
    }

    [HttpGet("/admin/media/library-items")]
    public async Task<IActionResult> LibraryItems(CancellationToken cancellationToken)
    {
        var items = await _adminMediaLibraryService.GetLatestItemsAsync(cancellationToken: cancellationToken);
        return Json(items);
    }

    [HttpPost("/admin/media/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? slug, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _adminMediaLibraryService.UploadAsync(file, slug, cancellationToken);
            return Json(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Medya yukleme sirasinda beklenmedik hata.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Medya yuklenemedi." });
        }
    }

    [HttpPost("/admin/media/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(MediaAssetUpdateInputModel model, CancellationToken cancellationToken)
    {
        var updated = await _adminMediaLibraryService.UpdateAsync(model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Medya guncellendi", "Baslik ve alt text kaydedildi.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/media/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _adminMediaLibraryService.DeleteAsync(id, cancellationToken);
            SetRedirectToast("success", "Medya silindi", "Kayit kutuphaneden kaldirildi.");
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminToast.Type"] = "warning";
            TempData["AdminToast.Title"] = "Medya silinemedi";
            TempData["AdminToast.Message"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
