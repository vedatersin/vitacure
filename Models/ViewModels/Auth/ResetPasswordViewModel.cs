using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Auth;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    [Display(Name = "Yeni Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Yeni Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
