using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;

namespace vitacure.Infrastructure.Services;

public class LocalAssetStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalAssetStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<AssetStorageUploadResult> UploadAsync(IFormFile file, string? slug, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsDirectory = Path.Combine(_environment.WebRootPath, "img", "library");
        Directory.CreateDirectory(uploadsDirectory);

        var safeSlug = NormalizeFileSegment(slug);
        var fileName = $"{safeSlug}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return new AssetStorageUploadResult
        {
            Url = $"/img/library/{fileName}",
            StorageKey = fileName,
            Provider = AssetStorageProvider.Local.ToString()
        };
    }

    public Task DeleteAsync(MediaAsset asset, CancellationToken cancellationToken = default)
    {
        var relativePath = !string.IsNullOrWhiteSpace(asset.StorageKey)
            ? Path.Combine("img", "library", asset.StorageKey)
            : asset.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string NormalizeFileSegment(string? value)
    {
        var raw = string.IsNullOrWhiteSpace(value) ? "asset" : value.Trim().ToLowerInvariant();
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(raw.Select(character => invalidChars.Contains(character) ? '-' : character).ToArray());

        return sanitized
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}
