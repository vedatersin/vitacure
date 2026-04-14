using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminTagService : IAdminTagService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;

    public AdminTagService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<TagListViewModel> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _dbContext.Tags
            .AsNoTracking()
            .Include(x => x.ProductTags)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = tags.Select(tag => new TagListItemViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            ProductCount = tag.ProductTags.Count
        }).ToList();

        return new TagListViewModel
        {
            TotalCount = items.Count,
            UsedCount = items.Count(x => x.ProductCount > 0),
            Tags = items
        };
    }

    public Task<TagFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TagFormViewModel());
    }

    public async Task<TagFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await _dbContext.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tag is null)
        {
            return null;
        }

        return new TagFormViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug
        };
    }

    public async Task<int> CreateAsync(TagFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = new Tag
        {
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim()
        };

        _dbContext.Tags.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(TagFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Tags.FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Name = model.Name.Trim();
        entity.Slug = model.Slug.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateCategoryAsync(cancellationToken);
        return true;
    }
}
