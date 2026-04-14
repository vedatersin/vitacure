using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminCategoryServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_Returns_Category_Counts_And_Product_Counts()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Root", Slug = "root", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Child", Slug = "child", Description = "B", ParentId = 1, IsActive = false });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Product A",
            Slug = "product-a",
            Description = "A",
            Price = 100m,
            Rating = 4.0m,
            ImageUrl = "/img/a.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new AdminCategoryService(dbContext, new FakeCacheInvalidationService());

        var result = await service.GetCategoriesAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.RootCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.Categories.First(x => x.Id == 1).ProductCount);
        Assert.Equal("Root", result.Categories.First(x => x.Id == 2).ParentName);
    }

    [Fact]
    public async Task CreateAsync_Persists_New_Category()
    {
        await using var dbContext = CreateDbContext();
        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminCategoryService(dbContext, cacheInvalidation);

        var id = await service.CreateAsync(new CategoryFormViewModel
        {
            Name = "Yeni Kategori",
            Slug = "yeni-kategori",
            Description = "Test",
            IsActive = true
        });

        var created = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Yeni Kategori", created!.Name);
        Assert.Equal("yeni-kategori", created.Slug);
        Assert.Equal(1, cacheInvalidation.CategoryInvalidationCount);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeCacheInvalidationService : Application.Abstractions.ICacheInvalidationService
    {
        public int CategoryInvalidationCount { get; private set; }

        public Task InvalidateStorefrontAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateCategoryAsync(CancellationToken cancellationToken = default)
        {
            CategoryInvalidationCount++;
            return Task.CompletedTask;
        }

        public Task InvalidateProductAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
