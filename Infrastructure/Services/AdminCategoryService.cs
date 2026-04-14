using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminCategoryService : IAdminCategoryService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;

    public AdminCategoryService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<CategoryListViewModel> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Include(x => x.Parent)
            .Include(x => x.Products)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = categories.Select(category => new CategoryListItemViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentName = category.Parent?.Name ?? "-",
            IsActive = category.IsActive,
            ProductCount = category.Products.Count
        }).ToList();

        return new CategoryListViewModel
        {
            TotalCount = items.Count,
            RootCount = categories.Count(x => x.ParentId is null),
            ActiveCount = categories.Count(x => x.IsActive),
            Categories = items
        };
    }

    public async Task<CategoryFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return new CategoryFormViewModel
        {
            ParentOptions = await GetParentOptionsAsync(cancellationToken)
        };
    }

    public async Task<CategoryFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        return new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentId = category.ParentId,
            SeoTitle = category.SeoTitle,
            MetaDescription = category.MetaDescription,
            IsActive = category.IsActive,
            ParentOptions = await GetParentOptionsAsync(cancellationToken, category.Id)
        };
    }

    public async Task<int> CreateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = new Category
        {
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            Description = model.Description.Trim(),
            ParentId = model.ParentId,
            SeoTitle = string.IsNullOrWhiteSpace(model.SeoTitle) ? null : model.SeoTitle.Trim(),
            MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? null : model.MetaDescription.Trim(),
            IsActive = model.IsActive
        };

        _dbContext.Categories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(CategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Name = model.Name.Trim();
        entity.Slug = model.Slug.Trim();
        entity.Description = model.Description.Trim();
        entity.ParentId = model.ParentId;
        entity.SeoTitle = string.IsNullOrWhiteSpace(model.SeoTitle) ? null : model.SeoTitle.Trim();
        entity.MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? null : model.MetaDescription.Trim();
        entity.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<CategoryOptionViewModel>> GetParentOptionsAsync(CancellationToken cancellationToken, int? excludeId = null)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(x => excludeId == null || x.Id != excludeId.Value)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryOptionViewModel
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);
    }
}
