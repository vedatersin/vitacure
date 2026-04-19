using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminMediaLibraryService : IAdminMediaLibraryService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif"
    };

    private readonly AppDbContext _dbContext;
    private readonly IAssetStorageService _assetStorageService;

    public AdminMediaLibraryService(AppDbContext dbContext, IAssetStorageService assetStorageService)
    {
        _dbContext = dbContext;
        _assetStorageService = assetStorageService;
    }

    public async Task<MediaLibraryViewModel> GetLibraryAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.MediaAssets
            .AsNoTracking()
            .Select(x => new
            {
                Entity = x,
                UsageCount = _dbContext.ProductMedias.Count(pm => pm.MediaAssetId == x.Id)
            })
            .OrderByDescending(x => x.Entity.CreatedAt)
            .ThenByDescending(x => x.Entity.Id)
            .ToListAsync(cancellationToken);
        var items = entities.Select(x => MapItem(x.Entity, x.UsageCount)).ToList();

        return new MediaLibraryViewModel
        {
            TotalCount = items.Count,
            TotalSizeBytes = items.Sum(x => x.SizeBytes),
            Items = items
        };
    }

    public async Task<IReadOnlyList<MediaAssetListItemViewModel>> GetLatestItemsAsync(int take = 48, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.MediaAssets
            .AsNoTracking()
            .Select(x => new
            {
                Entity = x,
                UsageCount = _dbContext.ProductMedias.Count(pm => pm.MediaAssetId == x.Id)
            })
            .OrderByDescending(x => x.Entity.CreatedAt)
            .ThenByDescending(x => x.Entity.Id)
            .Take(Math.Clamp(take, 1, 96))
            .ToListAsync(cancellationToken);

        return entities.Select(x => MapItem(x.Entity, x.UsageCount)).ToList();
    }

    public async Task<MediaAssetListItemViewModel> UploadAsync(IFormFile file, string? slug, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Gorsel secilmedi.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Desteklenmeyen gorsel formati.");
        }

        var uploadResult = await _assetStorageService.UploadAsync(file, slug, cancellationToken);

        var entity = new MediaAsset
        {
            FileName = Path.GetFileName(uploadResult.StorageKey),
            OriginalFileName = Path.GetFileName(file.FileName),
            Title = Path.GetFileNameWithoutExtension(file.FileName),
            StorageProvider = uploadResult.Provider,
            StorageKey = uploadResult.StorageKey,
            Url = uploadResult.Url,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            SizeBytes = file.Length
        };

        _dbContext.MediaAssets.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(entity, 0);
    }

    public async Task<bool> UpdateAsync(MediaAssetUpdateInputModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();
        entity.AltText = string.IsNullOrWhiteSpace(model.AltText) ? null : model.AltText.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        var isInUse = await _dbContext.ProductMedias.AnyAsync(x => x.MediaAssetId == id, cancellationToken);
        if (isInUse)
        {
            throw new InvalidOperationException("Bu medya bir urunde kullaniliyor. Once urun baglantisini kaldirin.");
        }

        await _assetStorageService.DeleteAsync(entity, cancellationToken);

        _dbContext.MediaAssets.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static MediaAssetListItemViewModel MapItem(MediaAsset entity, int usageCount)
    {
        return new MediaAssetListItemViewModel
        {
            Id = entity.Id,
            FileName = entity.FileName,
            OriginalFileName = entity.OriginalFileName,
            Title = entity.Title,
            AltText = entity.AltText,
            Url = entity.Url,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            SizeLabel = FormatSize(entity.SizeBytes),
            CreatedAt = entity.CreatedAt,
            UsageCount = usageCount
        };
    }

    private static string FormatSize(long sizeBytes)
    {
        if (sizeBytes < 1024)
        {
            return $"{sizeBytes} B";
        }

        if (sizeBytes < 1024 * 1024)
        {
            return $"{sizeBytes / 1024d:0.#} KB";
        }

        return $"{sizeBytes / 1024d / 1024d:0.#} MB";
    }
}
