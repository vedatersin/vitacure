using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application.Abstractions;
using vitacure.Domain.Enums;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class SystemController : AdminControllerBase
{
    private readonly IAdminStorageSettingsService _adminStorageSettingsService;
    private readonly ILogger<SystemController> _logger;

    public SystemController(IAdminStorageSettingsService adminStorageSettingsService, ILogger<SystemController> logger)
    {
        _adminStorageSettingsService = adminStorageSettingsService;
        _logger = logger;
    }

    [HttpGet("/admin/system/storage")]
    public async Task<IActionResult> Storage(CancellationToken cancellationToken)
    {
        var model = await _adminStorageSettingsService.GetModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/system/storage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Storage(StorageSettingsFormViewModel model, CancellationToken cancellationToken)
    {
        ValidateStorageModel(model);

        if (!ModelState.IsValid)
        {
            SetValidationToast("Dosya ve CDN ayarlari guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminStorageSettingsService.UpdateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla guncellendi", "Dosya ve CDN ayarlari kaydedildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya ve CDN ayarlari guncellenirken beklenmedik hata.");
            SetUnexpectedErrorToast("Dosya ve CDN ayarlari guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Storage));
    }

    private void ValidateStorageModel(StorageSettingsFormViewModel model)
    {
        if (!model.IsCdnEnabled || model.Provider == AssetStorageProvider.Local)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(model.ServiceUrl))
        {
            ModelState.AddModelError(nameof(model.ServiceUrl), "Service URL zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(model.BucketName))
        {
            ModelState.AddModelError(nameof(model.BucketName), "Bucket veya zone bilgisi zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(model.AccessKey))
        {
            ModelState.AddModelError(nameof(model.AccessKey), "Access key zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(model.SecretKey))
        {
            ModelState.AddModelError(nameof(model.SecretKey), "Secret key zorunludur.");
        }
    }
}
