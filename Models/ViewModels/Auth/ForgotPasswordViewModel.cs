using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Auth;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
    public string ResetUrl { get; set; } = string.Empty;
}
