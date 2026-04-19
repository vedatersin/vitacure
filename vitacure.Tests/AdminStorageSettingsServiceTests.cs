using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Tests;

public class AdminStorageSettingsServiceTests
{
    [Fact]
    public async Task UpdateAsync_Persists_S3_Compatible_Settings()
    {
        await using var dbContext = CreateDbContext();
        var service = new AdminStorageSettingsService(dbContext);

        await service.UpdateAsync(new StorageSettingsFormViewModel
        {
            Provider = AssetStorageProvider.S3Compatible,
            IsCdnEnabled = true,
            ServiceUrl = "https://example.r2.cloudflarestorage.com",
            PublicBaseUrl = "https://cdn.example.com",
            BucketName = "vitacure-media",
            Region = "auto",
            AccessKey = "test-access",
            SecretKey = "test-secret",
            KeyPrefix = "library",
            UsePathStyle = true
        });

        var entity = await dbContext.StorageSettings.FirstAsync();
        Assert.Equal(AssetStorageProvider.S3Compatible, entity.Provider);
        Assert.True(entity.IsCdnEnabled);
        Assert.Equal("https://cdn.example.com", entity.PublicBaseUrl);
        Assert.Equal("vitacure-media", entity.BucketName);
    }

    [Fact]
    public async Task GetModelAsync_Returns_Local_Default_When_No_Record_Exists()
    {
        await using var dbContext = CreateDbContext();
        var service = new AdminStorageSettingsService(dbContext);

        var model = await service.GetModelAsync();

        Assert.Equal(AssetStorageProvider.Local, model.Provider);
        Assert.False(model.IsCdnEnabled);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
