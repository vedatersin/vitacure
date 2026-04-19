using System.ComponentModel.DataAnnotations;
using vitacure.Domain.Enums;

namespace vitacure.Models.ViewModels.Admin;

public class StorageSettingsFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Storage Provider")]
    public AssetStorageProvider Provider { get; set; } = AssetStorageProvider.Local;

    [Display(Name = "CDN Aktif")]
    public bool IsCdnEnabled { get; set; }

    [Display(Name = "Service URL")]
    public string? ServiceUrl { get; set; }

    [Display(Name = "Public Base URL")]
    public string? PublicBaseUrl { get; set; }

    [Display(Name = "Bucket / Zone")]
    public string? BucketName { get; set; }

    [Display(Name = "Region")]
    public string? Region { get; set; }

    [Display(Name = "Access Key")]
    public string? AccessKey { get; set; }

    [Display(Name = "Secret Key")]
    public string? SecretKey { get; set; }

    [Display(Name = "Prefix")]
    public string? KeyPrefix { get; set; }

    [Display(Name = "Path Style")]
    public bool UsePathStyle { get; set; } = true;

    public string ActiveProviderLabel => !IsCdnEnabled || Provider == AssetStorageProvider.Local
        ? "Local"
        : "S3 Uyumlu CDN";
}
