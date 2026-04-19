using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminFeatureService : IAdminFeatureService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;
    private readonly ISlugService _slugService;

    public AdminFeatureService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService, ISlugService slugService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _slugService = slugService;
    }

    public async Task<FeatureListViewModel> GetFeaturesAsync(CancellationToken cancellationToken = default)
    {
        var features = await _dbContext.Features
            .AsNoTracking()
            .Include(x => x.ProductFeatures)
            .OrderBy(x => x.GroupName)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = features.Select(feature => new FeatureListItemViewModel
        {
            Id = feature.Id,
            Name = feature.Name,
            Slug = feature.Slug,
            GroupName = feature.GroupName,
            OptionsPreview = BuildPreview(feature.OptionsContent),
            IsActive = feature.IsActive,
            ProductCount = feature.ProductFeatures.Count
        }).ToList();

        return new FeatureListViewModel
        {
            TotalCount = items.Count,
            ActiveCount = items.Count(x => x.IsActive),
            UsedCount = items.Count(x => x.ProductCount > 0),
            Features = items
        };
    }

    public Task<FeatureFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new FeatureFormViewModel());

    public async Task<FeatureFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var feature = await _dbContext.Features
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (feature is null)
        {
            return null;
        }

        return new FeatureFormViewModel
        {
            Id = feature.Id,
            Name = feature.Name,
            Slug = feature.Slug,
            GroupName = feature.GroupName,
            OptionsContent = feature.OptionsContent,
            IsActive = feature.IsActive
        };
    }

    public async Task<int> CreateAsync(FeatureFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Feature, cancellationToken: cancellationToken);

        var entity = new Feature
        {
            Name = model.Name.Trim(),
            Slug = normalizedSlug,
            GroupName = model.GroupName.Trim(),
            OptionsContent = NormalizeOptions(model.OptionsContent),
            IsActive = model.IsActive
        };

        _dbContext.Features.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(FeatureFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Features.FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Feature, entity.Id, cancellationToken);

        entity.Name = model.Name.Trim();
        entity.Slug = normalizedSlug;
        entity.GroupName = model.GroupName.Trim();
        entity.OptionsContent = NormalizeOptions(model.OptionsContent);
        entity.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return true;
    }

    private static string? NormalizeOptions(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var lines = value
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return lines.Length == 0 ? null : string.Join(Environment.NewLine, lines);
    }

    private static string? BuildPreview(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var firstThree = value
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(3)
            .ToArray();

        return firstThree.Length == 0 ? null : string.Join(", ", firstThree);
    }
}
