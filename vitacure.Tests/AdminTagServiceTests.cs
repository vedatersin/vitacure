using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminTagServiceTests
{
    [Fact]
    public async Task GetTagsAsync_Returns_Counts_And_Product_Usage()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Tags.AddRange(
            new Tag { Id = 1, Name = "Bağışıklık", Slug = "bagisiklik" },
            new Tag { Id = 2, Name = "Uyku", Slug = "uyku" });

        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Kategori",
            Slug = "kategori",
            Description = "A",
            IsActive = true
        });

        dbContext.Products.Add(new Product
        {
            Id = 10,
            Name = "Ürün",
            Slug = "urun",
            Description = "A",
            Price = 100m,
            Rating = 4.5m,
            ImageUrl = "/img/a.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });

        dbContext.ProductTags.Add(new ProductTag
        {
            ProductId = 10,
            TagId = 1
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminTagService(dbContext, new FakeCacheInvalidationService());

        var result = await service.GetTagsAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.UsedCount);
        Assert.Equal(1, result.Tags.First(x => x.Id == 1).ProductCount);
    }

    [Fact]
    public async Task CreateAsync_Persists_New_Tag()
    {
        await using var dbContext = CreateDbContext();
        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminTagService(dbContext, cacheInvalidation);

        var id = await service.CreateAsync(new TagFormViewModel
        {
            Name = "Yeni Etiket",
            Slug = "yeni-etiket"
        });

        var created = await dbContext.Tags.FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Yeni Etiket", created!.Name);
        Assert.Equal("yeni-etiket", created.Slug);
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
