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

        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext);

        var result = await service.GetProductsAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.OutOfStockCount);
        Assert.Equal("Uyku", result.Products.First(x => x.Id == 1).CategoryName);
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
        dbContext.Tags.AddRange(
            new Tag { Id = 10, Name = "Bağışıklık", Slug = "bagisiklik" },
            new Tag { Id = 11, Name = "Enerji", Slug = "enerji" });
        await dbContext.SaveChangesAsync();

        var service = new AdminProductService(dbContext);

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
            CategoryId = 1,
            IsActive = true,
            SelectedTagIds = new[] { 10, 11 }
        });

        var created = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        Assert.NotNull(created);
        Assert.Equal("Yeni Ürün", created!.Name);
        Assert.Equal("yeni-urun", created.Slug);
        Assert.Equal(1, created.CategoryId);
        Assert.Equal(2, await dbContext.ProductTags.CountAsync(x => x.ProductId == id));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
