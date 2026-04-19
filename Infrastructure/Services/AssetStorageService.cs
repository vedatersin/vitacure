using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;

namespace vitacure.Infrastructure.Services;

public class AssetStorageService : IAssetStorageService
{
    private readonly AppDbContext _dbContext;
    private readonly LocalAssetStorageService _localAssetStorageService;
    private readonly S3CompatibleAssetStorageService _s3CompatibleAssetStorageService;

    public AssetStorageService(
        AppDbContext dbContext,
        LocalAssetStorageService localAssetStorageService,
        S3CompatibleAssetStorageService s3CompatibleAssetStorageService)
    {
        _dbContext = dbContext;
        _localAssetStorageService = localAssetStorageService;
        _s3CompatibleAssetStorageService = s3CompatibleAssetStorageService;
    }

    public async Task<AssetStorageUploadResult> UploadAsync(IFormFile file, string? slug, CancellationToken cancellationToken = default)
    {
        var settings = await GetCurrentSettingsAsync(cancellationToken);
        if (settings is null || !settings.IsCdnEnabled || settings.Provider == AssetStorageProvider.Local)
        {
            return await _localAssetStorageService.UploadAsync(file, slug, cancellationToken);
        }

        return settings.Provider switch
        {
            AssetStorageProvider.S3Compatible => await _s3CompatibleAssetStorageService.UploadAsync(file, slug, settings, cancellationToken),
            _ => await _localAssetStorageService.UploadAsync(file, slug, cancellationToken)
        };
    }

    public async Task DeleteAsync(MediaAsset asset, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AssetStorageProvider>(asset.StorageProvider, out var provider))
        {
            provider = AssetStorageProvider.Local;
        }

        if (provider == AssetStorageProvider.Local)
        {
            await _localAssetStorageService.DeleteAsync(asset, cancellationToken);
            return;
        }

        var settings = await GetCurrentSettingsAsync(cancellationToken);
        if (settings is null)
        {
            throw new InvalidOperationException("Aktif storage ayari bulunamadi.");
        }

        switch (provider)
        {
            case AssetStorageProvider.S3Compatible:
                await _s3CompatibleAssetStorageService.DeleteAsync(asset, settings, cancellationToken);
                break;
            default:
                await _localAssetStorageService.DeleteAsync(asset, cancellationToken);
                break;
        }
    }

    private Task<StorageSettings?> GetCurrentSettingsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.StorageSettings
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
