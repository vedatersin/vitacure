using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminFeatureServiceTests
{
    [Fact]
    public async Task GetFeaturesAsync_Returns_Counts_And_Product_Usage()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Features.AddRange(
            new Feature { Id = 1, Name = "Urun Formu", Slug = "urun-formu", GroupName = "Form", IsActive = true },
            new Feature { Id = 2, Name = "Hedef Destek", Slug = "hedef-destek", GroupName = "Hedef", IsActive = false });

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

        dbContext.ProductFeatures.Add(new ProductFeature
        {
            ProductId = 10,
            FeatureId = 1
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminFeatureService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));
        var result = await service.GetFeaturesAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.UsedCount);
        Assert.Equal(1, result.Features.First(x => x.Id == 1).ProductCount);
    }

    [Fact]
    public async Task CreateAsync_Persists_New_Feature()
    {
        await using var dbContext = CreateDbContext();
        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminFeatureService(dbContext, cacheInvalidation, new SlugService(dbContext));

        var id = await service.CreateAsync(new FeatureFormViewModel
        {
            Name = "Icerik Tipi",
            Slug = "icerik-tipi",
            GroupName = "Icerik",
            OptionsContent = "Vitamin\nMineral",
            IsActive = true
        });

        var created = await dbContext.Features.FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Icerik Tipi", created!.Name);
        Assert.Equal("Icerik", created.GroupName);
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
