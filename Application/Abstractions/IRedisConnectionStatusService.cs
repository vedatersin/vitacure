using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IRedisConnectionStatusService
{
    Task<RedisConnectionStatusViewModel> GetStatusAsync(CancellationToken cancellationToken = default);
}
