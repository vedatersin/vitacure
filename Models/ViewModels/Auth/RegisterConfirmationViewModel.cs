namespace vitacure.Models.ViewModels.Auth;

public class RegisterConfirmationViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ConfirmationUrl { get; set; } = string.Empty;
}
