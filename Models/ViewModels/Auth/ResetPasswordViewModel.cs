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
    [Display(Name = "Yeni Sifre")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Yeni Sifre Tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
