using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel.DataAnnotations;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminShowcaseServiceTests
{
    [Fact]
    public async Task GetShowcasesAsync_Returns_Counts_And_Home_Visibility()
    {
        var webRoot = CreateWebRoot();

        await using var dbContext = CreateDbContext();
        SeedCatalog(dbContext);
        dbContext.Showcases.AddRange(
            new Showcase
            {
                Id = 1,
                Name = "Uyku Sagligi",
                Slug = "uyku-sagligi",
                Title = "Uyku Sagligi",
                Description = "Test",
                BackgroundImageUrl = "/img/uykuBg.png",
                IsActive = true,
                ShowOnHome = true,
                ShowcaseCategories = new List<ShowcaseCategory> { new() { CategoryId = 1 } }
            },
            new Showcase
            {
                Id = 2,
                Name = "Enerji",
                Slug = "enerji",
                Title = "Enerji",
                Description = "Test",
                BackgroundImageUrl = "/img/multivitaminBg.png",
                IsActive = false,
                ShowOnHome = false
            });
        await dbContext.SaveChangesAsync();

        var service = new AdminShowcaseService(dbContext, new FakeCacheInvalidationService(), new FakeWebHostEnvironment(webRoot), new SlugService(dbContext));

        var result = await service.GetShowcasesAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.ActiveCount);
        Assert.Equal(1, result.HomeVisibleCount);
        Assert.Contains(result.Showcases, x => x.Slug == "uyku-sagligi" && x.CategoryCount == 1);
    }

    [Fact]
    public async Task CreateAsync_Persists_Showcase_With_Categories_And_Featured_Products()
    {
        var webRoot = CreateWebRoot();

        await using var dbContext = CreateDbContext();
        SeedCatalog(dbContext);
        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminShowcaseService(dbContext, cacheInvalidation, new FakeWebHostEnvironment(webRoot), new SlugService(dbContext));

        var id = await service.CreateAsync(new ShowcaseFormViewModel
        {
            Name = "Uyku Vitrini",
            Slug = "uyku-vitrini",
            Title = "Uyku Vitrini",
            Description = "Uyku odakli vitrin",
            TagsContent = "Melatonin",
            BackgroundImageUrl = "/img/uykuBg.png",
            IsActive = true,
            ShowOnHome = true,
            SortOrder = 1,
            SelectedCategoryIds = new List<int> { 1, 2 },
            SelectedFeaturedProductIds = new List<int> { 1, 2 }
        });

        var created = await dbContext.Showcases
            .Include(x => x.ShowcaseCategories)
            .Include(x => x.FeaturedProducts)
            .FirstOrDefaultAsync(x => x.Id == id);

        Assert.NotNull(created);
        Assert.Equal("Uyku Vitrini", created!.Name);
        Assert.True(created.ShowOnHome);
        Assert.True(created.IsDark);
        Assert.Equal(2, created.ShowcaseCategories.Count);
        Assert.Equal(2, created.FeaturedProducts.Count);
        Assert.Equal(1, cacheInvalidation.StorefrontInvalidationCount);
        Assert.Equal("/img/uykuBg.png", created.BackgroundImageUrl);
    }

    [Fact]
    public async Task GetCreateModelAsync_Returns_Background_Options_With_Recommended_Image()
    {
        var webRoot = CreateWebRoot();

        await using var dbContext = CreateDbContext();
        SeedCatalog(dbContext);
        var service = new AdminShowcaseService(dbContext, new FakeCacheInvalidationService(), new FakeWebHostEnvironment(webRoot), new SlugService(dbContext));

        var model = await service.GetCreateModelAsync();

        Assert.NotEmpty(model.BackgroundOptions);
        Assert.Contains(model.BackgroundOptions, option => option.ImageUrl == "/img/uykuBg.png");
    }

    [Fact]
    public async Task GetCreateModelAsync_Prefills_Featured_Product_Slots_From_Active_Product_List()
    {
        var webRoot = CreateWebRoot();

        await using var dbContext = CreateDbContext();
        SeedCatalog(dbContext);
        var service = new AdminShowcaseService(dbContext, new FakeCacheInvalidationService(), new FakeWebHostEnvironment(webRoot), new SlugService(dbContext));

        var model = await service.GetCreateModelAsync();

        Assert.NotEmpty(model.ProductOptions);
        Assert.NotEmpty(model.SelectedFeaturedProductIds);
        Assert.True(model.SelectedFeaturedProductIds.Count <= 7);
        Assert.Equal(model.SelectedFeaturedProductIds.Distinct().Count(), model.SelectedFeaturedProductIds.Count);
        Assert.All(model.SelectedFeaturedProductIds, productId => Assert.Contains(model.ProductOptions, option => option.Id == productId));
    }

    [Fact]
    public void ShowcaseFormValidation_Allows_Submit_When_Background_File_Is_Selected()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var formFile = new FormFile(stream, 0, stream.Length, "BackgroundImageFile", "preview.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var model = new ShowcaseFormViewModel
        {
            Name = "Uyku Vitrini",
            Slug = "uyku-vitrini",
            IconClass = "fa-solid fa-sparkles",
            Title = "Uyku Vitrini",
            Description = "Uyku odakli vitrin",
            BackgroundImageFile = formFile,
            BackgroundImageUrl = string.Empty
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, validateAllProperties: true);

        Assert.True(isValid);
        Assert.DoesNotContain(validationResults, result => result.MemberNames.Contains(nameof(ShowcaseFormViewModel.BackgroundImageUrl)));
    }

    [Fact]
    public void ShowcaseFormValidation_Fails_When_Description_Exceeds_Max_Length()
    {
        var model = new ShowcaseFormViewModel
        {
            Name = "Uyku Vitrini",
            Slug = "uyku-vitrini",
            IconClass = "fa-solid fa-sparkles",
            Title = "Uyku Vitrini",
            Description = new string('a', ShowcaseFormViewModel.MaxDescriptionLength + 1),
            BackgroundImageUrl = "/img/uykuBg.png"
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(ShowcaseFormViewModel.Description)));
    }

    [Fact]
    public async Task UpdateAsync_Preserves_Existing_Categories_And_Featured_Products_When_Only_Background_Is_Changed()
    {
        var webRoot = CreateWebRoot();

        await using var dbContext = CreateDbContext();
        SeedCatalog(dbContext);
        dbContext.Showcases.Add(new Showcase
        {
            Id = 15,
            Name = "Uyku Vitrini",
            Slug = "uyku-vitrini",
            Title = "Uyku Vitrini",
            Description = "Uyku odakli vitrin",
            TagsContent = "Melatonin",
            BackgroundImageUrl = "/img/uykuBg.png",
            IsDark = false,
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
                new() { ProductId = 1, SortOrder = 0 },
                new() { ProductId = 2, SortOrder = 1 }
            }
        });
        await dbContext.SaveChangesAsync();

        var cacheInvalidation = new FakeCacheInvalidationService();
        var service = new AdminShowcaseService(dbContext, cacheInvalidation, new FakeWebHostEnvironment(webRoot), new SlugService(dbContext));

        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var formFile = new FormFile(stream, 0, stream.Length, "BackgroundImageFile", "updated.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var updated = await service.UpdateAsync(new ShowcaseFormViewModel
        {
            Id = 15,
            Name = "Uyku Vitrini",
            Slug = "uyku-vitrini",
            IconClass = "fa-solid fa-moon",
            Title = "Uyku Vitrini",
            Description = "Uyku odakli vitrin",
            TagsContent = "Melatonin",
            BackgroundImageUrl = string.Empty,
            BackgroundImageFile = formFile,
            IsDark = false,
            IsActive = true,
            ShowOnHome = true,
            SortOrder = 1,
            SelectedCategoryIds = new List<int>(),
            SelectedFeaturedProductIds = new List<int>()
        });

        var showcase = await dbContext.Showcases
            .Include(x => x.ShowcaseCategories)
            .Include(x => x.FeaturedProducts)
            .SingleAsync(x => x.Id == 15);

        Assert.True(updated);
        Assert.False(showcase.IsDark);
        Assert.Single(showcase.ShowcaseCategories);
        Assert.Equal(1, showcase.ShowcaseCategories.Single().CategoryId);
        Assert.Equal(2, showcase.FeaturedProducts.Count);
        Assert.Contains(showcase.FeaturedProducts, x => x.ProductId == 1);
        Assert.Contains(showcase.FeaturedProducts, x => x.ProductId == 2);
        Assert.StartsWith("/img/showcases/uyku-vitrini-", showcase.BackgroundImageUrl, StringComparison.Ordinal);
    }


    private static void SeedCatalog(AppDbContext dbContext)
    {
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Uyku", Slug = "uyku", Description = "A", IsActive = true },
            new Category { Id = 2, Name = "Melatonin", Slug = "melatonin", Description = "B", ParentId = 1, IsActive = true });

        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Night Support",
                Slug = "night-support",
                Description = "A",
                Price = 100m,
                Rating = 4.5m,
                ImageUrl = "/img/a.png",
                Stock = 10,
                CategoryId = 1,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                Name = "Melatonin Plus",
                Slug = "melatonin-plus",
                Description = "B",
                Price = 110m,
                Rating = 4.7m,
                ImageUrl = "/img/b.png",
                Stock = 12,
                CategoryId = 2,
                IsActive = true
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

    private static string CreateWebRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "vitacure-showcase-tests", Guid.NewGuid().ToString("N"));
        var img = Path.Combine(root, "img");
        Directory.CreateDirectory(img);
        File.WriteAllText(Path.Combine(img, "uykuBg.png"), "stub");
        File.WriteAllText(Path.Combine(img, "multivitaminBg.png"), "stub");
        File.WriteAllText(Path.Combine(img, "zekaHafızaBg.png"), "stub");
        return root;
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

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string webRootPath)
        {
            ApplicationName = "vitacure.Tests";
            WebRootPath = webRootPath;
            WebRootFileProvider = new PhysicalFileProvider(webRootPath);
            ContentRootPath = webRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(webRootPath);
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
