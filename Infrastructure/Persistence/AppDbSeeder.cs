using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;

namespace vitacure.Infrastructure.Persistence;

public class AppDbSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public AppDbSeeder(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Categories.AnyAsync(cancellationToken) || await _dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var document = await LoadDocumentAsync(cancellationToken);
        var categories = BuildCategories(document);
        await _dbContext.Categories.AddRangeAsync(categories, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var categoryMap = await _dbContext.Categories
            .ToDictionaryAsync(x => x.Slug, x => x.Id, cancellationToken);

        var uncategorizedCategoryId = categoryMap["uncategorized"];
        var products = BuildProducts(document, categoryMap, uncategorizedCategoryId);

        await _dbContext.Products.AddRangeAsync(products, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<MockSeedDocument> LoadDocumentAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(_environment.ContentRootPath, "docs", "mock-data.json");
        await using var stream = File.OpenRead(path);
        var document = await JsonSerializer.DeserializeAsync<MockSeedDocument>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken);

        return document ?? new MockSeedDocument();
    }

    private static List<Category> BuildCategories(MockSeedDocument document)
    {
        var categories = document.Categories
            .Where(x => !string.IsNullOrWhiteSpace(x.SlugCandidate))
            .Select(x => new Category
            {
                Name = x.Name,
                Slug = x.SlugCandidate,
                Description = x.Description ?? string.Empty,
                MetaDescription = x.Description,
                SeoTitle = $"{x.Name} | VitaCure",
                IsActive = true
            })
            .ToList();

        categories.Add(new Category
        {
            Name = "Uncategorized",
            Slug = "uncategorized",
            Description = "Primary category is not assigned yet.",
            MetaDescription = "Primary category is not assigned yet.",
            SeoTitle = "Uncategorized | VitaCure",
            IsActive = true
        });

        return categories
            .GroupBy(x => x.Slug, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();
    }

    private static List<Product> BuildProducts(
        MockSeedDocument document,
        IReadOnlyDictionary<string, int> categoryMap,
        int uncategorizedCategoryId)
    {
        return document.Products
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new Product
            {
                Name = x.Name,
                Slug = Slugify(x.Name),
                Description = x.Description ?? string.Empty,
                Price = ParseDecimal(x.Price),
                OldPrice = ParseNullableDecimal(x.OldPrice),
                Rating = ParseDecimal(x.Rating),
                ImageUrl = x.Image ?? string.Empty,
                Stock = 100,
                CategoryId = ResolveCategoryId(x.CategoryRelation, categoryMap, uncategorizedCategoryId),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .GroupBy(x => x.Slug, StringComparer.OrdinalIgnoreCase)
            .Select((group, index) =>
            {
                var product = group.First();
                if (group.Count() > 1)
                {
                    product.Slug = $"{product.Slug}-{index + 1}";
                }

                return product;
            })
            .ToList();
    }

    private static int ResolveCategoryId(
        IReadOnlyList<string>? categoryRelations,
        IReadOnlyDictionary<string, int> categoryMap,
        int uncategorizedCategoryId)
    {
        if (categoryRelations is null)
        {
            return uncategorizedCategoryId;
        }

        foreach (var slug in categoryRelations)
        {
            if (categoryMap.TryGetValue(slug, out var categoryId))
            {
                return categoryId;
            }
        }

        return uncategorizedCategoryId;
    }

    private static decimal ParseDecimal(string? value)
    {
        return decimal.TryParse(
            value?.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var result)
            ? result
            : 0m;
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : ParseDecimal(value);
    }

    private static string Slugify(string value)
    {
        return value.Trim().ToLowerInvariant()
            .Replace("ç", "c")
            .Replace("ğ", "g")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ş", "s")
            .Replace("ü", "u")
            .Replace("&", string.Empty)
            .Replace("+", "plus")
            .Replace("  ", " ")
            .Replace(" ", "-");
    }

    private sealed class MockSeedDocument
    {
        public List<MockCategoryItem> Categories { get; set; } = new();
        public List<MockProductItem> Products { get; set; } = new();
    }

    private sealed class MockCategoryItem
    {
        public string Name { get; set; } = string.Empty;
        public string SlugCandidate { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    private sealed class MockProductItem
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Price { get; set; }
        public string? OldPrice { get; set; }
        public string? Rating { get; set; }
        public string? Image { get; set; }
        public List<string> CategoryRelation { get; set; } = new();
    }
}
