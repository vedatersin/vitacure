using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;

namespace vitacure.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetActiveCategoriesAsync_Returns_Only_Active_Categories_Ordered_By_Name()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category { Id = 1, Name = "Z Category", Slug = "z-category", Description = "Z", IsActive = true },
            new Category { Id = 2, Name = "A Category", Slug = "a-category", Description = "A", IsActive = true },
            new Category { Id = 3, Name = "Inactive Category", Slug = "inactive-category", Description = "I", IsActive = false });

        await dbContext.SaveChangesAsync();

        var service = new CategoryService(dbContext);

        var result = await service.GetActiveCategoriesAsync();

        var categories = result.ToList();
        Assert.Equal(2, categories.Count);
        Assert.Equal("A Category", categories[0].Name);
        Assert.Equal("Z Category", categories[1].Name);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
