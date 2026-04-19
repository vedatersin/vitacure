using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminProductServiceTests
{
    [Fact]
    public async Task GetProductsAsync_Returns_Counts_And_Category_Labels()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Uyku", Slug = "uyku", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Enerji", Slug = "enerji", Description = "B", IsActive = true });
        dbContext.Brands.Add(new Brand
        {
            Id = 5,
            Name = "Solgar",
            Slug = "solgar",
            IsActive = true
        });
        dbContext.Features.Add(new Feature
        {
            Id = 30,
            Name = "Urun Formu",
            Slug = "urun-formu",
            GroupName = "Form",
            IsActive = true
        });

        dbContext.Tags.Add(new Tag
        {
            Id = 20,
            Name = "Destek",
            Slug = "destek"
        });

        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Melatonin",
                Slug = "melatonin",
                Description = "Uyku desteği",
                Price = 199m,
                Rating = 4.5m,
                ImageUrl = "/img/melatonin.png",
                Stock = 0,
                BrandId = 5,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                Name = "B12",
                Slug = "b12",
                Description = "Enerji desteği",
                Price = 149m,
                Rating = 4.2m,
                ImageUrl = "/img/b12.png",
                Stock = 12,
                CategoryId = 2,
                IsActive = false
            });

        dbContext.ProductTags.Add(new ProductTag
        {
            ProductId = 1,
            TagId = 20
        });
        dbContext.ProductFeatures.Add(new ProductFeature
        {
            ProductId = 1,
            FeatureId = 30,
            Value = "Kapsul"
        });

        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var result = await service.GetProductsAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.OutOfStockCount);
        Assert.Equal("Solgar", result.Products.First(x => x.Id == 1).BrandName);
        Assert.Equal("Uyku", result.Products.First(x => x.Id == 1).CategoryName);
        Assert.Equal(1, result.Products.First(x => x.Id == 1).FeatureCount);
        Assert.Equal(1, result.Products.First(x => x.Id == 1).TagCount);
    }

    [Fact]
    public async Task CreateAsync_Persists_New_Product_With_Tag_Assignments()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku",
            Slug = "uyku",
            Description = "A",
            IsActive = true
        });
        dbContext.Brands.Add(new Brand
        {
            Id = 7,
            Name = "Ocean",
            Slug = "ocean",
            IsActive = true
        });
        dbContext.Features.Add(new Feature
        {
            Id = 25,
            Name = "Hedef Destek",
            Slug = "hedef-destek",
            GroupName = "Hedef",
            IsActive = true
        });
        dbContext.Tags.AddRange(
            new Tag { Id = 10, Name = "Bağışıklık", Slug = "bagisiklik" },
            new Tag { Id = 11, Name = "Enerji", Slug = "enerji" });
        await dbContext.SaveChangesAsync();

        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminProductService(dbContext, cacheInvalidation, new SlugService(dbContext));

        var id = await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Yeni Ürün",
            Slug = "yeni-urun",
            Description = "Test açıklaması",
            Price = 249m,
            OldPrice = 299m,
            Rating = 4.7m,
            ImageUrl = "/img/yeni-urun.png",
            Stock = 25,
            BrandId = 7,
            CategoryId = 1,
            IsActive = true,
            SelectedFeatureValues = new Dictionary<int, string> { [25] = "Uyku" },
            SelectedFeatureIds = new[] { 25 },
            SelectedTagIds = new[] { 10, 11 }
        });

        var created = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Yeni Ürün", created!.Name);
        Assert.Equal("yeni-urun", created.Slug);
        Assert.Equal(7, created.BrandId);
        Assert.Equal(1, created.CategoryId);
        Assert.Equal(1, await dbContext.ProductFeatures.CountAsync(x => x.ProductId == id));
        Assert.Equal("Uyku", (await dbContext.ProductFeatures.FirstAsync(x => x.ProductId == id && x.FeatureId == 25)).Value);
        Assert.Equal(2, await dbContext.ProductTags.CountAsync(x => x.ProductId == id));
        Assert.Equal(1, cacheInvalidation.ProductInvalidationCount);
    }

    [Fact]
    public async Task CreateAsync_Persists_Normalized_Product_Media_And_Syncs_Legacy_Fields()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku",
            Slug = "uyku",
            Description = "A",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var id = await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Medyali Urun",
            Slug = "medyali-urun",
            Description = "Test",
            Price = 100m,
            Rating = 4.1m,
            ImageUrl = "/img/cover.png",
            GalleryImageUrls = "/img/gallery-1.png\n/img/gallery-2.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });

        var created = await dbContext.Products
            .Include(x => x.ProductMedias.OrderBy(media => media.SortOrder))
            .FirstAsync(x => x.Id == id);

        Assert.Equal("/img/cover.png", created.ImageUrl);
        Assert.Equal("/img/gallery-1.png" + Environment.NewLine + "/img/gallery-2.png", created.GalleryImageUrls);
        Assert.Equal(3, created.ProductMedias.Count);
        Assert.Equal("/img/cover.png", created.ProductMedias.First(x => x.IsPrimary).Url);
    }

    [Fact]
    public async Task CreateAsync_Links_Product_Media_To_Selected_MediaAssets()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku",
            Slug = "uyku",
            Description = "A",
            IsActive = true
        });
        dbContext.MediaAssets.Add(new MediaAsset
        {
            Id = 11,
            FileName = "asset.png",
            OriginalFileName = "asset.png",
            StorageProvider = "Local",
            StorageKey = "asset.png",
            Url = "/img/library/asset.png",
            ContentType = "image/png",
            SizeBytes = 1024,
            AltText = "Kutuphane gorseli"
        });
        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var id = await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Bagli Medya Urunu",
            Slug = "bagli-medya-urunu",
            Description = "Test",
            Price = 100m,
            Rating = 4.1m,
            ImageUrl = "/img/library/asset.png",
            MediaItemsJson = """[{"url":"/img/library/asset.png","assetId":11}]""",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });

        var created = await dbContext.Products
            .Include(x => x.ProductMedias)
            .FirstAsync(x => x.Id == id);

        var media = Assert.Single(created.ProductMedias);
        Assert.Equal(11, media.MediaAssetId);
        Assert.Equal("Kutuphane gorseli", media.AltText);
    }

    [Fact]
    public async Task CreateAsync_Persists_Product_Variants_And_Uses_First_Active_Variant_For_Summary()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku",
            Slug = "uyku",
            Description = "A",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var id = await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Melatonin Kompleks",
            Slug = "melatonin-kompleks",
            Description = "Test aciklamasi",
            Price = 10m,
            Rating = 4.5m,
            ImageUrl = "/img/melatonin.png",
            Stock = 1,
            CategoryId = 1,
            IsActive = true,
            Variants = new[]
            {
                new ProductVariantInputViewModel
                {
                    GroupName = "Boyut",
                    OptionName = "30 Tablet",
                    Price = 189m,
                    OldPrice = 219m,
                    Stock = 8,
                    SortOrder = 0,
                    IsActive = true
                },
                new ProductVariantInputViewModel
                {
                    GroupName = "Boyut",
                    OptionName = "60 Tablet",
                    Price = 299m,
                    OldPrice = 349m,
                    Stock = 12,
                    SortOrder = 1,
                    IsActive = true
                }
            }
        });

        var created = await dbContext.Products
            .Include(x => x.ProductVariants)
            .FirstAsync(x => x.Id == id);

        Assert.Equal(2, created.ProductVariants.Count);
        Assert.Equal(189m, created.Price);
        Assert.Equal(219m, created.OldPrice);
        Assert.Equal(20, created.Stock);
    }

    [Fact]
    public async Task CreateAsync_Persists_ProductCategory_Relations()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Uyku", Slug = "uyku", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Mineral", Slug = "mineral", Description = "B", IsActive = true },
            new Category { Id = 3, Name = "Gece Rutini", Slug = "gece-rutini", Description = "C", IsActive = true });
        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        var id = await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Yeni Urun",
            Slug = "yeni-urun",
            Description = "Test",
            Price = 100m,
            Rating = 4.2m,
            ImageUrl = "/img/a.png",
            Stock = 5,
            CategoryId = 1,
            SelectedCategoryIds = new[] { 2, 3 },
            IsActive = true
        });

        var relations = await dbContext.ProductCategories
            .Where(x => x.ProductId == id)
            .OrderBy(x => x.CategoryId)
            .Select(x => x.CategoryId)
            .ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, relations);
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Slug_Is_Reserved()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku",
            Slug = "uyku",
            Description = "A",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext, new FakeCacheInvalidationService(), new SlugService(dbContext));

        await Assert.ThrowsAsync<vitacure.Application.SlugConflictException>(() => service.CreateAsync(new ProductFormViewModel
        {
            Name = "Yeni Urun",
            Slug = "login",
            Description = "Test aciklamasi",
            Price = 249m,
            Rating = 4.7m,
            ImageUrl = "/img/yeni-urun.png",
            Stock = 25,
            CategoryId = 1,
            IsActive = true
        }));
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
        public int ProductInvalidationCount { get; private set; }

        public Task InvalidateStorefrontAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task InvalidateCategoryAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateProductAsync(CancellationToken cancellationToken = default)
        {
            ProductInvalidationCount++;
            return Task.CompletedTask;
        }
    }
}
