using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

public abstract class AdminControllerBase : Controller
{
    private const string ToastKey = "AdminToast";

    protected void SetPageToast(string type, string title, string message, IEnumerable<string>? details = null, bool isSticky = true)
    {
        ViewData[ToastKey] = BuildToast(type, title, message, details, isSticky);
    }

    protected void SetRedirectToast(string type, string title, string message, IEnumerable<string>? details = null, bool isSticky = false)
    {
        TempData[ToastKey] = JsonSerializer.Serialize(BuildToast(type, title, message, details, isSticky));
    }

    protected void SetValidationToast(string title, string fallbackMessage = "Lutfen form alanlarini kontrol edin.")
    {
        var details = ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => NormalizeValidationMessage(string.IsNullOrWhiteSpace(error.ErrorMessage) ? fallbackMessage : error.ErrorMessage.Trim()))
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .Take(6)
            .ToArray();

        var message = details.FirstOrDefault() ?? fallbackMessage;
        SetPageToast("warning", title, message, details.Skip(1).ToArray(), isSticky: true);
    }

    protected void SetUnexpectedErrorToast(string title, Exception exception)
    {
        var details = new[] { exception.Message.Trim() }
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToArray();

        SetPageToast("error", title, "Kayit sirasinda beklenmedik bir hata olustu.", details, isSticky: true);
    }

    private static AdminToastViewModel BuildToast(string type, string title, string message, IEnumerable<string>? details, bool isSticky)
    {
        return new AdminToastViewModel
        {
            Type = string.IsNullOrWhiteSpace(type) ? "info" : type.Trim().ToLowerInvariant(),
            Title = title.Trim(),
            Message = message.Trim(),
            Details = details?
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>(),
            IsSticky = isSticky
        };
    }

    private static string NormalizeValidationMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Lutfen form alanlarini kontrol edin.";
        }

        var requiredMatch = System.Text.RegularExpressions.Regex.Match(message, "^The (?<field>.+) field is required\\.$");
        if (requiredMatch.Success)
        {
            return $"{requiredMatch.Groups["field"].Value.Trim()} alani zorunludur.";
        }

        return message.Trim();
    }
}
