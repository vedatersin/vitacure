using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetBySlugAsync_Returns_Active_Product_With_Category()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku Sagligi",
            Slug = "uyku-sagligi",
            Description = "Test category",
            IsActive = true
        });
        dbContext.Tags.Add(new Tag
        {
            Id = 99,
            Name = "Mineral",
            Slug = "mineral"
        });

        dbContext.Products.Add(new Product
        {
            Id = 10,
            Name = "Magnezyum",
            Slug = "magnezyum",
            Description = "Test product",
            Price = 249m,
            OldPrice = 349m,
            Rating = 4.3m,
            ImageUrl = "/img/products/bottle_mag_nobg.png",
            Stock = 100,
            CategoryId = 1,
            IsActive = true
        });
        dbContext.ProductTags.Add(new ProductTag
        {
            ProductId = 10,
            TagId = 99
        });
        dbContext.ProductMedias.AddRange(
            new ProductMedia
            {
                Id = 1,
                ProductId = 10,
                Url = "/img/products/bottle_mag_nobg.png",
                SortOrder = 0,
                IsPrimary = true
            },
            new ProductMedia
            {
                Id = 2,
                ProductId = 10,
                Url = "/img/products/bottle_mag_detail.png",
                SortOrder = 1,
                IsPrimary = false
            });
        dbContext.ProductVariants.AddRange(
            new ProductVariant
            {
                Id = 1,
                ProductId = 10,
                GroupName = "Boyut",
                OptionName = "30 Tablet",
                Price = 249m,
                OldPrice = 349m,
                Stock = 10,
                SortOrder = 0,
                IsActive = true
            },
            new ProductVariant
            {
                Id = 2,
                ProductId = 10,
                GroupName = "Boyut",
                OptionName = "60 Tablet",
                Price = 399m,
                Stock = 5,
                SortOrder = 1,
                IsActive = true
            });

        await dbContext.SaveChangesAsync();

        var service = new ProductService(dbContext);

        var result = await service.GetBySlugAsync("magnezyum");

        Assert.NotNull(result);
        Assert.Equal("Magnezyum", result.Name);
        Assert.NotNull(result.Category);
        Assert.Equal("uyku-sagligi", result.Category!.Slug);
        Assert.Single(result.ProductTags);
        Assert.Equal("mineral", result.ProductTags.First().Tag!.Slug);
        Assert.Equal(2, result.ProductVariants.Count);
        Assert.Equal(2, result.ProductMedias.Count);
    }

    [Fact]
    public async Task GetRelatedProductsAsync_Filters_By_Category_And_Excludes_Current_Product()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Uyku Sagligi", Slug = "uyku-sagligi", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Enerji", Slug = "enerji", Description = "B", IsActive = true });

        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Current Product",
                Slug = "current-product",
                Description = "Current",
                Price = 100m,
                Rating = 4.5m,
                ImageUrl = "/img/current.png",
                Stock = 10,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                Name = "Same Category Product",
                Slug = "same-category-product",
                Description = "Related",
                Price = 120m,
                Rating = 4.9m,
                ImageUrl = "/img/related.png",
                Stock = 10,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 3,
                Name = "Other Category Product",
                Slug = "other-category-product",
                Description = "Other",
                Price = 99m,
                Rating = 4.8m,
                ImageUrl = "/img/other.png",
                Stock = 10,
                CategoryId = 2,
                IsActive = true
            });

        await dbContext.SaveChangesAsync();

        var service = new ProductService(dbContext);

        var result = await service.GetRelatedProductsAsync(1, 1);

        var relatedProducts = result.ToList();
        Assert.Single(relatedProducts);
        Assert.Equal("same-category-product", relatedProducts[0].Slug);
    }

    [Fact]
    public async Task GetProductsByCategorySlugAsync_Returns_Products_From_Secondary_Category_Relation()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Ana", Slug = "ana", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Ek", Slug = "ek", Description = "B", IsActive = true });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Multi Product",
            Slug = "multi-product",
            Description = "Multi",
            Price = 100m,
            Rating = 4.1m,
            ImageUrl = "/img/multi.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });
        dbContext.ProductCategories.Add(new ProductCategory
        {
            ProductId = 1,
            CategoryId = 2
        });
        await dbContext.SaveChangesAsync();

        var service = new ProductService(dbContext);

        var result = await service.GetProductsByCategorySlugAsync("ek");

        Assert.Single(result);
        Assert.Equal("multi-product", result[0].Slug);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
