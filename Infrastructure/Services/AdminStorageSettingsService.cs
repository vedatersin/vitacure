using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminStorageSettingsService : IAdminStorageSettingsService
{
    private readonly AppDbContext _dbContext;

    public AdminStorageSettingsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StorageSettingsFormViewModel> GetModelAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.StorageSettings
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return new StorageSettingsFormViewModel();
        }

        return new StorageSettingsFormViewModel
        {
            Id = entity.Id,
            Provider = entity.Provider,
            IsCdnEnabled = entity.IsCdnEnabled,
            ServiceUrl = entity.ServiceUrl,
            PublicBaseUrl = entity.PublicBaseUrl,
            BucketName = entity.BucketName,
            Region = entity.Region,
            AccessKey = entity.AccessKey,
            SecretKey = entity.SecretKey,
            KeyPrefix = entity.KeyPrefix,
            UsePathStyle = entity.UsePathStyle
        };
    }

    public async Task UpdateAsync(StorageSettingsFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.StorageSettings
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        entity ??= new StorageSettings();
        entity.Provider = model.IsCdnEnabled ? model.Provider : AssetStorageProvider.Local;
        entity.IsCdnEnabled = model.IsCdnEnabled && model.Provider != AssetStorageProvider.Local;
        entity.ServiceUrl = Clean(model.ServiceUrl);
        entity.PublicBaseUrl = Clean(model.PublicBaseUrl);
        entity.BucketName = Clean(model.BucketName);
        entity.Region = Clean(model.Region);
        entity.AccessKey = Clean(model.AccessKey);
        entity.SecretKey = Clean(model.SecretKey);
        entity.KeyPrefix = Clean(model.KeyPrefix);
        entity.UsePathStyle = model.UsePathStyle;
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.Id == 0)
        {
            _dbContext.StorageSettings.Add(entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
