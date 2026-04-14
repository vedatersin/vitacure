namespace vitacure.Models.ViewModels.Admin;

public class RedisConnectionStatusViewModel
{
    public bool IsConfigured { get; set; }
    public bool IsConnected { get; set; }
    public string Title { get; set; } = "Redis";
    public string StatusLabel { get; set; } = "Yapılandırılmadı";
    public string Detail { get; set; } = "Redis connection string tanımlı değil.";
}
