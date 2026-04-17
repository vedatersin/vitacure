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
            Assert.Equal("/daily-multivitamin", result.FeaturedProducts[0].Href);
            Assert.Equal("/", result.CanonicalPath);
            Assert.Equal("Öne Çıkan Ürünler", result.SectionHeaders["featured"].Title);
            Assert.Equal("Vitacure test aciklamasi", result.MetaDescription);
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
            Assert.Equal("/daily-multivitamin", result!.CanonicalPath);
            Assert.Equal(3, result.Breadcrumbs.Count);
            Assert.NotEmpty(result.RelatedProducts);
            Assert.Contains("Bagisiklik", result.Tags);
            Assert.All(result.RelatedProducts, product => Assert.StartsWith("/", product.Href, StringComparison.Ordinal));
            Assert.DoesNotContain(result.RelatedProducts, product => product.Href.StartsWith("/urun/", StringComparison.Ordinal));
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
            Assert.Single(result.CoverflowProducts.Select(x => x.Id).Distinct());
            Assert.Single(result.CoverflowProducts);
            Assert.All(result.CoverflowProducts, product => Assert.Equal("Night Support", product.Name));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetHomePageContentAsync_Uses_Db_Home_Content_When_Configured()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedHomeData(dbContext);
            dbContext.HomeContentSettings.Add(new HomeContentSettings
            {
                MetaDescription = "Db meta description",
                HeroTitle = "DB Hero",
                HeroSubtitle = "DB Subtitle",
                MainPlaceholder = "DB main placeholder",
                SearchPlaceholder = "DB search",
                SearchPlaceholderLocked = "DB locked search",
                FeaturedTitle = "DB Featured",
                FeaturedActionLabel = "Hepsini Gor",
                FeaturedActionUrl = "/tum-urunler",
                PopularTitle = "DB Popular",
                CampaignsTitle = "DB Campaigns",
                DealsTitle = "DB Deals",
                DealsActionLabel = "Tum Firsatlar",
                DealsActionUrl = "/firsatlar",
                FeaturedBannerName = "DB Banner",
                FeaturedBannerAltText = "DB Banner Alt",
                FeaturedBannerImageUrl = "/img/db-banner.png",
                FeaturedBannerTargetUrl = "/vitacure-ai",
                PopularSupplementsContent = "Demir | /img/demir.png | /kategori/demir | linear-gradient(135deg,#111,#222) | #fefefe",
                CampaignBannersContent = "/img/banner-1.jpg | /kampanya/1 | Kampanya 1"
            });
            dbContext.SaveChanges();

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetHomePageContentAsync();

            Assert.Equal("Db meta description", result.MetaDescription);
            Assert.Equal("DB Hero", result.ChatWidget.HeroTitle);
            Assert.Equal("DB Featured", result.SectionHeaders["featured"].Title);
            Assert.Equal("/vitacure-ai", result.FeaturedBanner!.TargetUrl);
            Assert.Single(result.PopularSupplements);
            Assert.Single(result.CampaignBanners);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetShowcasePageContentAsync_Preserves_Custom_Background_Image()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedShowcaseData(dbContext, "/img/showcases/custom-sleep.png");

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetShowcasePageContentAsync("uyku-sagligi");

            Assert.NotNull(result);
            Assert.Equal("/img/showcases/custom-sleep.png", result!.Showcase.BackgroundImageUrl);
            Assert.True(result.Showcase.IsDark);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetShowcasePageContentAsync_Returns_Showcase_Light_Mode()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedShowcaseData(dbContext, "/img/showcases/custom-sleep.png", isDark: false);

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetShowcasePageContentAsync("uyku-sagligi");

            Assert.NotNull(result);
            Assert.False(result!.Showcase.IsDark);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetHomePageContentAsync_Uses_Showcase_Custom_Background_And_Theme_Pill()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedHomeData(dbContext);
            dbContext.Showcases.Add(new Showcase
            {
                Id = 50,
                Name = "Uyku Sagligi",
                Slug = "uyku-sagligi",
                Title = "Uyku Sagligi",
                Description = "Uyku vitrin aciklamasi",
                BackgroundImageUrl = "/img/showcases/custom-home.png",
                IconClass = "fa-solid fa-moon",
                IsActive = true,
                ShowOnHome = true,
                SortOrder = 1,
                ShowcaseCategories = new List<ShowcaseCategory>
                {
                    new() { CategoryId = 1 }
                }
            });
            dbContext.SaveChanges();

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetHomePageContentAsync();
            var showcase = Assert.Single(result.Showcases);

            Assert.Equal("/img/showcases/custom-home.png", showcase.BackgroundImageUrl);
            Assert.Equal("bg-uyku", showcase.PillCssClass);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetShowcasePageContentAsync_Falls_Back_To_Slug_Category_When_Showcase_Relations_Are_Missing()
    {
        var contentRoot = TestContentRootFactory.Create();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedHomeData(dbContext);
            dbContext.Showcases.Add(new Showcase
            {
                Id = 60,
                Name = "Uyku Sagligi",
                Slug = "uyku-rutini",
                Title = "Uyku Sagligi",
                Description = "Uyku vitrin aciklamasi",
                BackgroundImageUrl = "/img/showcases/custom-fallback.png",
                IconClass = "fa-solid fa-moon",
                IsActive = true,
                ShowOnHome = true,
                SortOrder = 1
            });
            dbContext.SaveChanges();

            var service = CreateStorefrontContentService(dbContext, contentRoot);

            var result = await service.GetShowcasePageContentAsync("uyku-rutini");

            Assert.NotNull(result);
            Assert.Single(result!.ProductGrid);
            Assert.Single(result.FeaturedProducts);
            Assert.Equal("/img/showcases/custom-fallback.png", result.Showcase.BackgroundImageUrl);
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
        var homeContentConfigurationService = new HomeContentConfigurationService(dbContext, environment);
        return new StorefrontContentService(dbContext, environment, productService, homeContentConfigurationService);
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
            ImageUrl = "/img/products/bottle_multi_nobg.png",
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
                ImageUrl = "/img/products/bottle_multi_nobg.png",
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
                ImageUrl = "/img/products/bottle_omega_nobg.png",
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

    private static void SeedShowcaseData(AppDbContext dbContext, string backgroundImageUrl, bool isDark = true)
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
            Name = "Night Support",
            Slug = "night-support",
            Description = "Night support",
            Price = 220m,
            Rating = 4.6m,
            ImageUrl = "/img/night-support.png",
            Stock = 50,
            CategoryId = 1,
            IsActive = true
        });

        dbContext.Showcases.Add(new Showcase
        {
            Id = 25,
            Name = "Uyku Sagligi",
            Slug = "uyku-sagligi",
            Title = "Uyku Sagligi",
            Description = "Uyku vitrin aciklamasi",
            BackgroundImageUrl = backgroundImageUrl,
            IsDark = isDark,
            IconClass = "fa-solid fa-moon",
            IsActive = true,
            ShowOnHome = true,
            SortOrder = 1,
            ShowcaseCategories = new List<ShowcaseCategory>
            {
                new() { CategoryId = 1 }
            },
            FeaturedProducts = new List<ShowcaseFeaturedProduct>
            {
                new() { ProductId = 1, SortOrder = 0 }
            }
        });

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
      "image": "/img/products/bottle_multi_nobg.png"
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
