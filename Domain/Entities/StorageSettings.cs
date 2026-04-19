using vitacure.Domain.Enums;

namespace vitacure.Domain.Entities;

public class StorageSettings
{
    public int Id { get; set; }
    public AssetStorageProvider Provider { get; set; } = AssetStorageProvider.Local;
    public bool IsCdnEnabled { get; set; }
    public string? ServiceUrl { get; set; }
    public string? PublicBaseUrl { get; set; }
    public string? BucketName { get; set; }
    public string? Region { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? KeyPrefix { get; set; }
    public bool UsePathStyle { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
