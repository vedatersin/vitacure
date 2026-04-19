using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminBrandServiceTests
{
    [Fact]
    public async Task GetBrandsAsync_Returns_Counts_And_Product_Usage()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Brands.AddRange(
            new Brand { Id = 1, Name = "Solgar", Slug = "solgar", IsActive = true },
            new Brand { Id = 2, Name = "Ocean", Slug = "ocean", IsActive = false });

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
            BrandId = 1,
            CategoryId = 1,
            IsActive = true
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminBrandService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var result = await service.GetBrandsAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.UsedCount);
        Assert.Equal(1, result.Brands.First(x => x.Id == 1).ProductCount);
    }

    [Fact]
    public async Task CreateAsync_Persists_New_Brand()
    {
        await using var dbContext = CreateDbContext();
        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminBrandService(dbContext, cacheInvalidation, new SlugService(dbContext));

        var id = await service.CreateAsync(new BrandFormViewModel
        {
            Name = "Yeni Marka",
            Slug = "yeni-marka",
            Description = "Test aciklamasi",
            IsActive = true
        });

        var created = await dbContext.Brands.FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Yeni Marka", created!.Name);
        Assert.Equal("yeni-marka", created.Slug);
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
        public Task InvalidateProductAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateCategoryAsync(CancellationToken cancellationToken = default)
        {
            CategoryInvalidationCount++;
            return Task.CompletedTask;
        }
    }
}
