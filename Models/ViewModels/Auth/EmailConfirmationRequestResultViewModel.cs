namespace vitacure.Models.ViewModels.Auth;

public class EmailConfirmationRequestResultViewModel
{
    public string Message { get; set; } = string.Empty;
    public string ConfirmationUrl { get; set; } = string.Empty;
}
