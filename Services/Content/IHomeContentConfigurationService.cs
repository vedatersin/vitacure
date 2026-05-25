using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public interface IHomeContentConfigurationService
{
    Task<HomeContentConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task<HomeContentConfiguration> GetFallbackConfigurationAsync(CancellationToken cancellationToken = default);
}
