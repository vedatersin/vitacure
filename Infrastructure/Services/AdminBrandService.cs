using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminBrandService : IAdminBrandService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;
    private readonly ISlugService _slugService;

    public AdminBrandService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService, ISlugService slugService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _slugService = slugService;
    }

    public async Task<BrandListViewModel> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _dbContext.Brands
            .AsNoTracking()
            .Include(x => x.Products)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = brands.Select(brand => new BrandListItemViewModel
        {
            Id = brand.Id,
            Name = brand.Name,
            Slug = brand.Slug,
            Description = brand.Description,
            IsActive = brand.IsActive,
            ProductCount = brand.Products.Count
        }).ToList();

        return new BrandListViewModel
        {
            TotalCount = items.Count,
            ActiveCount = items.Count(x => x.IsActive),
            UsedCount = items.Count(x => x.ProductCount > 0),
            Brands = items
        };
    }

    public Task<BrandFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new BrandFormViewModel());
    }

    public async Task<BrandFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await _dbContext.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (brand is null)
        {
            return null;
        }

        return new BrandFormViewModel
        {
            Id = brand.Id,
            Name = brand.Name,
            Slug = brand.Slug,
            Description = brand.Description,
            IsActive = brand.IsActive
        };
    }

    public async Task<int> CreateAsync(BrandFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Brand, cancellationToken: cancellationToken);

        var entity = new Brand
        {
            Name = model.Name.Trim(),
            Slug = normalizedSlug,
            Description = NormalizeDescription(model.Description),
            IsActive = model.IsActive
        };

        _dbContext.Brands.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(BrandFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Brands.FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Brand, entity.Id, cancellationToken);

        entity.Name = model.Name.Trim();
        entity.Slug = normalizedSlug;
        entity.Description = NormalizeDescription(model.Description);
        entity.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return true;
    }

    private static string? NormalizeDescription(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
