using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;

namespace vitacure.Tests;

public class AppDbSeederTests
{
    [Fact]
    public async Task SeedAsync_DoesNotOverwrite_Existing_Showcase_Customizations()
    {
        var contentRoot = CreateContentRoot();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedCategoryAndProduct(dbContext);

            dbContext.Showcases.Add(new Showcase
            {
                Id = 99,
                Name = "Uyku Sagligi",
                Slug = "uyku-sagligi",
                Title = "Ozel Vitrin Basligi",
                Description = "Ozel vitrin aciklamasi",
                TagsContent = "Ozel, Etiket",
                BackgroundImageUrl = "/img/showcases/custom-sleep.png",
                IconClass = "fa-solid fa-moon",
                SeoTitle = "Ozel SEO",
                MetaDescription = "Ozel meta",
                IsActive = true,
                ShowOnHome = false,
                SortOrder = 9,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ShowcaseCategories = new List<ShowcaseCategory>
                {
                    new() { CategoryId = 1 }
                },
                FeaturedProducts = new List<ShowcaseFeaturedProduct>
                {
                    new() { ProductId = 1, SortOrder = 0 }
                }
            });
            await dbContext.SaveChangesAsync();

            var seeder = new AppDbSeeder(dbContext, new FakeWebHostEnvironment(contentRoot));

            await seeder.SeedAsync();

            var showcase = await dbContext.Showcases
                .Include(x => x.ShowcaseCategories)
                .Include(x => x.FeaturedProducts)
                .SingleAsync(x => x.Id == 99);

            Assert.Equal("Ozel Vitrin Basligi", showcase.Title);
            Assert.Equal("Ozel vitrin aciklamasi", showcase.Description);
            Assert.Equal("Ozel, Etiket", showcase.TagsContent);
            Assert.Equal("/img/showcases/custom-sleep.png", showcase.BackgroundImageUrl);
            Assert.Equal("Ozel SEO", showcase.SeoTitle);
            Assert.Equal("Ozel meta", showcase.MetaDescription);
            Assert.False(showcase.ShowOnHome);
            Assert.Equal(9, showcase.SortOrder);
            Assert.Single(showcase.ShowcaseCategories);
            Assert.Single(showcase.FeaturedProducts);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SeedAsync_Repairs_Existing_Showcase_When_Relations_Are_Missing()
    {
        var contentRoot = CreateContentRoot();

        try
        {
            await using var dbContext = CreateDbContext();
            SeedCategoryAndProduct(dbContext);

            dbContext.Showcases.Add(new Showcase
            {
                Id = 100,
                Name = "Uyku Sagligi",
                Slug = "uyku-rutini",
                Title = "Uyku Sagligi",
                Description = "Mevcut aciklama",
                TagsContent = "Melatonin",
                BackgroundImageUrl = "/img/showcases/custom-sleep.png",
                IconClass = "fa-solid fa-moon",
                SeoTitle = "SEO",
                MetaDescription = "Meta",
                IsActive = true,
                ShowOnHome = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();

            var seeder = new AppDbSeeder(dbContext, new FakeWebHostEnvironment(contentRoot));

            await seeder.SeedAsync();

            var showcase = await dbContext.Showcases
                .Include(x => x.ShowcaseCategories)
                .Include(x => x.FeaturedProducts)
                .SingleAsync(x => x.Id == 100);

            Assert.Equal("/img/showcases/custom-sleep.png", showcase.BackgroundImageUrl);
            Assert.Single(showcase.ShowcaseCategories);
            Assert.Equal(1, showcase.ShowcaseCategories.Single().CategoryId);
            Assert.Single(showcase.FeaturedProducts);
            Assert.Equal(1, showcase.FeaturedProducts.Single().ProductId);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedCategoryAndProduct(AppDbContext dbContext)
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

        dbContext.SaveChanges();
    }

    private static string CreateContentRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "vitacure-seeder-tests", Guid.NewGuid().ToString("N"));
        var docs = Path.Combine(root, "docs");
        var img = Path.Combine(root, "wwwroot", "img");
        Directory.CreateDirectory(docs);
        Directory.CreateDirectory(img);

        File.WriteAllText(Path.Combine(docs, "mock-data.json"), """
{
  "categories": [],
  "products": []
}
""");

        File.WriteAllText(Path.Combine(img, "uykuBg.png"), "stub");

        return root;
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string contentRootPath)
        {
            ApplicationName = "vitacure.Tests";
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
            WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
            EnvironmentName = "Development";
        }

        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
    }
}
