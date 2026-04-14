namespace vitacure.Models.ViewModels.Auth;

public class PasswordResetRequestResultViewModel
{
    public string Message { get; set; } = string.Empty;
    public string ResetUrl { get; set; } = string.Empty;
}
