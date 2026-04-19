using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;

namespace vitacure.Infrastructure.Services;

public class S3CompatibleAssetStorageService
{
    public async Task<AssetStorageUploadResult> UploadAsync(IFormFile file, string? slug, StorageSettings settings, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeSlug = NormalizeFileSegment(slug);
        var keyPrefix = string.IsNullOrWhiteSpace(settings.KeyPrefix) ? "library" : settings.KeyPrefix.Trim().Trim('/');
        var objectKey = $"{keyPrefix}/{safeSlug}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";

        using var client = CreateClient(settings);
        await using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = settings.BucketName,
            Key = objectKey,
            InputStream = stream,
            AutoCloseStream = false,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType
        };
        await client.PutObjectAsync(request, cancellationToken);

        return new AssetStorageUploadResult
        {
            Url = BuildPublicUrl(settings, objectKey),
            StorageKey = objectKey,
            Provider = AssetStorageProvider.S3Compatible.ToString()
        };
    }

    public async Task DeleteAsync(MediaAsset asset, StorageSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(asset.StorageKey))
        {
            return;
        }

        using var client = CreateClient(settings);
        await client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = settings.BucketName,
            Key = asset.StorageKey
        }, cancellationToken);
    }

    private static AmazonS3Client CreateClient(StorageSettings settings)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = settings.ServiceUrl,
            ForcePathStyle = settings.UsePathStyle,
            AuthenticationRegion = string.IsNullOrWhiteSpace(settings.Region) ? "auto" : settings.Region
        };

        var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
        return new AmazonS3Client(credentials, config);
    }

    private static string BuildPublicUrl(StorageSettings settings, string objectKey)
    {
        if (!string.IsNullOrWhiteSpace(settings.PublicBaseUrl))
        {
            return $"{settings.PublicBaseUrl.TrimEnd('/')}/{objectKey}";
        }

        return $"{settings.ServiceUrl?.TrimEnd('/')}/{settings.BucketName}/{objectKey}";
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
