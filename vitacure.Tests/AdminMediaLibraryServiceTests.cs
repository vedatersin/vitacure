using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminMediaLibraryServiceTests
{
    [Fact]
    public async Task UploadAsync_Persists_MediaAsset_And_Writes_File()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "vitacure-media-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);

        try
        {
            await using var dbContext = CreateDbContext();
            var service = CreateService(dbContext, webRoot);

            await using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "sample.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            var result = await service.UploadAsync(file, "urun-gorseli");

            Assert.StartsWith("/img/library/urun-gorseli-", result.Url, StringComparison.Ordinal);
            Assert.Single(dbContext.MediaAssets);
            Assert.True(File.Exists(Path.Combine(webRoot, "img", "library", Path.GetFileName(result.Url))));
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetLatestItemsAsync_Returns_Newest_First()
    {
        await using var dbContext = CreateDbContext();
        dbContext.MediaAssets.AddRange(
            new MediaAsset
            {
                Id = 1,
                FileName = "old.png",
                OriginalFileName = "old.png",
                StorageProvider = "Local",
                StorageKey = "old.png",
                Url = "/img/library/old.png",
                ContentType = "image/png",
                SizeBytes = 1024,
                CreatedAt = new DateTime(2026, 4, 18, 10, 0, 0, DateTimeKind.Utc)
            },
            new MediaAsset
            {
                Id = 2,
                FileName = "new.png",
                OriginalFileName = "new.png",
                StorageProvider = "Local",
                StorageKey = "new.png",
                Url = "/img/library/new.png",
                ContentType = "image/png",
                SizeBytes = 2048,
                CreatedAt = new DateTime(2026, 4, 19, 10, 0, 0, DateTimeKind.Utc)
            });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, Path.GetTempPath());

        var items = await service.GetLatestItemsAsync();

        Assert.Equal(new[] { "new.png", "old.png" }, items.Select(x => x.OriginalFileName).ToArray());
    }

    [Fact]
    public async Task UpdateAsync_Persists_Title_And_AltText()
    {
        await using var dbContext = CreateDbContext();
        dbContext.MediaAssets.Add(new MediaAsset
        {
            Id = 5,
            FileName = "asset.png",
            OriginalFileName = "asset.png",
            StorageProvider = "Local",
            StorageKey = "asset.png",
            Url = "/img/library/asset.png",
            ContentType = "image/png",
            SizeBytes = 1024
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, Path.GetTempPath());

        var updated = await service.UpdateAsync(new MediaAssetUpdateInputModel
        {
            Id = 5,
            Title = "Hero Gorseli",
            AltText = "Uyku destek urun gorseli"
        });

        var entity = await dbContext.MediaAssets.FirstAsync(x => x.Id == 5);
        Assert.True(updated);
        Assert.Equal("Hero Gorseli", entity.Title);
        Assert.Equal("Uyku destek urun gorseli", entity.AltText);
    }

    [Fact]
    public async Task DeleteAsync_Throws_When_Asset_Is_In_Use()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Test Urun",
            Slug = "test-urun",
            Description = "A",
            Price = 10m,
            Rating = 4m,
            ImageUrl = "/img/a.png",
            Stock = 5,
            CategoryId = 1,
            IsActive = true
        });
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Kategori",
            Slug = "kategori",
            Description = "A",
            IsActive = true
        });
        dbContext.MediaAssets.Add(new MediaAsset
        {
            Id = 9,
            FileName = "used.png",
            OriginalFileName = "used.png",
            StorageProvider = "Local",
            StorageKey = "used.png",
            Url = "/img/library/used.png",
            ContentType = "image/png",
            SizeBytes = 1024
        });
        dbContext.ProductMedias.Add(new ProductMedia
        {
            ProductId = 1,
            MediaAssetId = 9,
            Url = "/img/library/used.png",
            SortOrder = 0,
            IsPrimary = true
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, Path.GetTempPath());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(9));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static AdminMediaLibraryService CreateService(AppDbContext dbContext, string webRoot)
    {
        var environment = new FakeWebHostEnvironment(webRoot);
        var assetStorageService = new AssetStorageService(
            dbContext,
            new LocalAssetStorageService(environment),
            new S3CompatibleAssetStorageService());
        return new AdminMediaLibraryService(dbContext, assetStorageService);
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string webRootPath)
        {
            ApplicationName = "vitacure.Tests";
            ContentRootPath = webRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(webRootPath);
            WebRootPath = webRootPath;
            WebRootFileProvider = new PhysicalFileProvider(webRootPath);
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
