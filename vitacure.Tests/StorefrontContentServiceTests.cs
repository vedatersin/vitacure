using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Services.Content;

namespace vitacure.Tests;

public class StorefrontContentServiceTests
{
    [Fact]
    public async Task GetHomePageContentAsync_Returns_Categories_And_Product_Links()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedHomeData(dbContext);

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetHomePageContentAsync();

            Assert.Single(result.Categories);
            Assert.NotEmpty(result.FeaturedProducts);
            Assert.Equal("/urun/daily-multivitamin", result.FeaturedProducts[0].Href);
            Assert.Equal("/", result.CanonicalPath);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetProductDetailPageContentAsync_Returns_Breadcrumbs_And_Related_Products()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedProductDetailData(dbContext);

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetProductDetailPageContentAsync("daily-multivitamin");

            Assert.NotNull(result);
            Assert.Equal("/urun/daily-multivitamin", result!.CanonicalPath);
            Assert.Equal(3, result.Breadcrumbs.Count);
            Assert.NotEmpty(result.RelatedProducts);
            Assert.Contains("Bagisiklik", result.Tags);
            Assert.All(result.RelatedProducts, product => Assert.StartsWith("/urun/", product.Href, StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetCategoryPageContentAsync_Filters_By_Tag_Query()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedCategoryTagData(dbContext);

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetCategoryPageContentAsync("uyku-sagligi", "melatonin");

            Assert.NotNull(result);
            Assert.Equal("Melatonin", result!.ActiveTagLabel);
            Assert.All(result.ProductGrid, product => Assert.Equal("Night Support", product.Name));
            Assert.Single(result.ProductGrid.Select(x => x.Id).Distinct());
            Assert.Contains(result.AvailableTags, x => x.Slug == "melatonin" && x.IsActive);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private static StorefrontContentService CreateStorefrontContentService(AppDbContext dbContext, string contentRoot)
    {
        var productService = new ProductService(dbContext);
        var environment = new FakeWebHostEnvironment(contentRoot);
        return new StorefrontContentService(dbContext, environment, productService);
    }

    private static void SeedHomeData(AppDbContext dbContext)
    {
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku Sagligi",
            Slug = "uyku-sagligi",
            Description = "Sleep support products",
            IsActive = true
        });

        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Daily Multivitamin",
            Slug = "daily-multivitamin",
            Description = "Daily support",
            Price = 299m,
            OldPrice = 399m,
            Rating = 4.8m,
            ImageUrl = "/img/bottle_multi_nobg.png",
            Stock = 100,
            CategoryId = 1,
            IsActive = true
        });

        dbContext.SaveChanges();
    }

    private static void SeedProductDetailData(AppDbContext dbContext)
    {
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku Sagligi",
            Slug = "uyku-sagligi",
            Description = "Sleep support products",
            IsActive = true
        });
        dbContext.Tags.Add(new Tag
        {
            Id = 10,
            Name = "Bagisiklik",
            Slug = "bagisiklik"
        });

        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Daily Multivitamin",
                Slug = "daily-multivitamin",
                Description = "Daily support",
                Price = 299m,
                OldPrice = 399m,
                Rating = 4.8m,
                ImageUrl = "/img/bottle_multi_nobg.png",
                Stock = 100,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                Name = "Omega 3",
                Slug = "omega-3",
                Description = "Omega support",
                Price = 349m,
                OldPrice = 499m,
                Rating = 4.9m,
                ImageUrl = "/img/bottle_omega_nobg.png",
                Stock = 100,
                CategoryId = 1,
                IsActive = true
            });
        dbContext.ProductTags.Add(new ProductTag
        {
            ProductId = 1,
            TagId = 10
        });

        dbContext.SaveChanges();
    }

    private static void SeedCategoryTagData(AppDbContext dbContext)
    {
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Uyku Sagligi",
            Slug = "uyku-sagligi",
            Description = "Sleep support products",
            IsActive = true
        });

        dbContext.Tags.AddRange(
            new Tag { Id = 1, Name = "Melatonin", Slug = "melatonin" },
            new Tag { Id = 2, Name = "Magnezyum", Slug = "magnezyum" });

        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Night Support",
                Slug = "night-support",
                Description = "Night support",
                Price = 220m,
                Rating = 4.6m,
                ImageUrl = "/img/night-support.png",
                Stock = 50,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                Name = "Calm Magnesium",
                Slug = "calm-magnesium",
                Description = "Calm support",
                Price = 210m,
                Rating = 4.4m,
                ImageUrl = "/img/calm-magnesium.png",
                Stock = 45,
                CategoryId = 1,
                IsActive = true
            });

        dbContext.ProductTags.AddRange(
            new ProductTag { ProductId = 1, TagId = 1 },
            new ProductTag { ProductId = 2, TagId = 2 });

        dbContext.SaveChanges();
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string contentRootPath)
        {
            ApplicationName = "vitacure.Tests";
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
            WebRootFileProvider = new PhysicalFileProvider(contentRootPath);
            EnvironmentName = "Development";
        }

        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
    }

    private static class TestContentRootFactory
    {
        public static string Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "vitacure-tests", Guid.NewGuid().ToString("N"));
            var docsPath = Path.Combine(root, "docs");
            Directory.CreateDirectory(docsPath);

            File.WriteAllText(Path.Combine(docsPath, "mock-data.json"), """
{
  "categories": [
    {
      "name": "Uyku Sagligi",
      "slugCandidate": "uyku-sagligi",
      "icon": "fa-moon"
    }
  ],
  "campaigns": [
    {
      "type": "popular-supplement-card",
      "name": "Vitaminler",
      "image": "/img/bottle_multi_nobg.png"
    }
  ],
  "banners": [
    {
      "imageFiles": [ "vitacureai.png" ]
    }
  ],
  "chatWidget": {
    "global": {
      "heroTitle": "Vitacure AI",
      "heroSubtitle": "Test subtitle",
      "compactBackLabel": "Geri",
      "compactCategoryLabel": "Kategori",
      "searchFilterLabel": "Kategori",
      "mainPlaceholder": "Urun veya semptom yazin",
      "fullscreenTitle": "Buyut",
      "addFileTitle": "Dosya ekle",
      "chatModeLabel": "Sohbet",
      "searchModeLabel": "Ara",
      "fileMenuDocumentLabel": "Dokuman",
      "fileMenuImageLabel": "Gorsel",
      "searchPlaceholder": "Ara",
      "searchPlaceholderLocked": "Kategori icinde ara"
    },
    "byCategory": {
      "uyku-sagligi": {
        "displayName": "Uyku Sagligi",
        "tagButtons": [ "Tumu", "Melatonin" ]
      }
    }
  },
  "examplePrompts": {
    "byCategory": {
      "uyku-sagligi": [ "Uyku icin ne onerirsin?" ]
    }
  },
  "filters": [
    {
      "group": "Sirala",
      "label": "Sirala:",
      "options": [ "Onerilen", "Fiyat" ]
    }
  ],
  "sections": [
    { "title": "One Cikan Urunler" },
    { "title": "Populer Takviyeler" },
    { "title": "Kampanyalar" },
    { "title": "Firsat Urunleri" }
  ],
  "seoCandidates": [ "Vitacure test aciklamasi" ]
}
""");

            return root;
        }
    }
}
