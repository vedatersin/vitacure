using StackExchange.Redis;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class RedisConnectionStatusService : IRedisConnectionStatusService
{
    private readonly IConfiguration _configuration;

    public RedisConnectionStatusService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<RedisConnectionStatusViewModel> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new RedisConnectionStatusViewModel();
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

            await using var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            var ping = await connection.GetDatabase().PingAsync();

            return new RedisConnectionStatusViewModel
            {
                IsConfigured = true,
                IsConnected = true,
                StatusLabel = "Bağlandı",
                Detail = $"Redis erişilebilir. Ping: {Math.Round(ping.TotalMilliseconds)} ms."
            };
        }
        catch (Exception ex)
        {
            return new RedisConnectionStatusViewModel
            {
                IsConfigured = true,
                IsConnected = false,
                StatusLabel = "Bağlanamadı",
                Detail = $"Bağlantı denemesi başarısız: {ex.Message}"
            };
        }
    }
}
