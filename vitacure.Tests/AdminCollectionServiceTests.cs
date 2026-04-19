using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminCollectionServiceTests
{
    [Fact]
    public async Task GetCollectionsAsync_Returns_Counts_And_Product_Usage()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Collections.AddRange(
            new Collection { Id = 1, Name = "Yaz Seckisi", Slug = "yaz-seckisi", IsActive = true, ShowOnHome = true },
            new Collection { Id = 2, Name = "Editor Secimi", Slug = "editor-secimi", IsActive = false, ShowOnHome = false });

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
            Name = "Urun",
            Slug = "urun",
            Description = "A",
            Price = 100m,
            Rating = 4.5m,
            ImageUrl = "/img/a.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });

        dbContext.ProductCollections.Add(new ProductCollection
        {
            ProductId = 10,
            CollectionId = 1
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminCollectionService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var result = await service.GetCollectionsAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.HomeVisibleCount);
        Assert.Equal(1, result.Collections.First(x => x.Id == 1).ProductCount);
    }

    [Fact]
    public async Task CreateAsync_Persists_New_Collection_With_Product_Relations()
    {
        await using var dbContext = CreateDbContext();
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
            Name = "Urun",
            Slug = "urun",
            Description = "A",
            Price = 100m,
            Rating = 4.5m,
            ImageUrl = "/img/a.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminCollectionService(dbContext, cacheInvalidation, new SlugService(dbContext));

        var id = await service.CreateAsync(new CollectionFormViewModel
        {
            Name = "Yeni Koleksiyon",
            Slug = "yeni-koleksiyon",
            Description = "Test aciklamasi",
            ShowOnHome = true,
            SortOrder = 2,
            IsActive = true,
            SelectedProductIds = new[] { 10 }
        });

        var created = await dbContext.Collections
            .Include(x => x.ProductCollections)
            .FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Yeni Koleksiyon", created!.Name);
        Assert.True(created.ShowOnHome);
        Assert.Single(created.ProductCollections);
        Assert.Equal(1, cacheInvalidation.StorefrontInvalidationCount);
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
        public int StorefrontInvalidationCount { get; private set; }

        public Task InvalidateStorefrontAsync(CancellationToken cancellationToken = default)
        {
            StorefrontInvalidationCount++;
            return Task.CompletedTask;
        }

        public Task InvalidateCategoryAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task InvalidateProductAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
