using Microsoft.AspNetCore.Http;
using vitacure.Domain.Entities;

namespace vitacure.Application.Abstractions;

public interface IAssetStorageService
{
    Task<AssetStorageUploadResult> UploadAsync(IFormFile file, string? slug, CancellationToken cancellationToken = default);
    Task DeleteAsync(MediaAsset asset, CancellationToken cancellationToken = default);
}

public sealed class AssetStorageUploadResult
{
    public string Url { get; init; } = string.Empty;
    public string StorageKey { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}
