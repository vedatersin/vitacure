using Microsoft.EntityFrameworkCore;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class SlugServiceTests
{
    [Fact]
    public async Task EnsureAvailableAsync_Throws_When_Slug_Is_Used_By_Another_Module()
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

        var service = new SlugService(dbContext);

        var exception = await Assert.ThrowsAsync<SlugConflictException>(() =>
            service.EnsureAvailableAsync("uyku", SlugEntityType.Product));

        Assert.Contains("kategori", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureAvailableAsync_Throws_When_Slug_Is_Used_By_Brand()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Brands.Add(new Brand
        {
            Id = 1,
            Name = "Solgar",
            Slug = "solgar",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new SlugService(dbContext);

        var exception = await Assert.ThrowsAsync<SlugConflictException>(() =>
            service.EnsureAvailableAsync("solgar", SlugEntityType.Product));

        Assert.Contains("marka", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureAvailableAsync_Throws_When_Slug_Is_Used_By_Feature()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Features.Add(new Feature
        {
            Id = 1,
            Name = "Urun Formu",
            Slug = "urun-formu",
            GroupName = "Form",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new SlugService(dbContext);

        var exception = await Assert.ThrowsAsync<SlugConflictException>(() =>
            service.EnsureAvailableAsync("urun-formu", SlugEntityType.Product));

        Assert.Contains("ozellik", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureAvailableAsync_Throws_When_Slug_Is_Reserved()
    {
        await using var dbContext = CreateDbContext();
        var service = new SlugService(dbContext);

        var exception = await Assert.ThrowsAsync<SlugConflictException>(() =>
            service.EnsureAvailableAsync("login", SlugEntityType.Tag));

        Assert.Contains("sistem route", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResolveStorefrontAsync_Prioritizes_Product_Then_Category()
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
        dbContext.Products.Add(new Product
        {
            Id = 10,
            Name = "Melatonin",
            Slug = "melatonin",
            Description = "B",
            Price = 99m,
            Rating = 4.4m,
            ImageUrl = "/img/melatonin.png",
            Stock = 10,
            CategoryId = 1,
            IsActive = true
        });
        dbContext.Showcases.Add(new Showcase
        {
            Id = 20,
            Name = "Uyku Rotasi",
            Slug = "uyku-rotasi",
            IconClass = "fa-solid fa-moon",
            Title = "Uyku Rotasi",
            Description = "A",
            BackgroundImageUrl = "/img/uykuBg.png",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = new SlugService(dbContext);

        var productMatch = await service.ResolveStorefrontAsync("melatonin");
        var showcaseMatch = await service.ResolveStorefrontAsync("uyku-rotasi");
        var categoryMatch = await service.ResolveStorefrontAsync("uyku");
        var noneMatch = await service.ResolveStorefrontAsync("olmayan");

        Assert.Equal(StorefrontSlugTargetType.Product, productMatch.TargetType);
        Assert.Equal(StorefrontSlugTargetType.Showcase, showcaseMatch.TargetType);
        Assert.Equal(StorefrontSlugTargetType.Category, categoryMatch.TargetType);
        Assert.Equal(StorefrontSlugTargetType.None, noneMatch.TargetType);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
