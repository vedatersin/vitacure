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
        var hasCategories = await _dbContext.Categories.AnyAsync(cancellationToken);
        var hasBrands = await _dbContext.Brands.AnyAsync(cancellationToken);
        var hasFeatures = await _dbContext.Features.AnyAsync(cancellationToken);
        var hasProducts = await _dbContext.Products.AnyAsync(cancellationToken);

        if (!hasCategories && !hasProducts)
        {
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

        if (!hasBrands)
        {
            await _dbContext.Brands.AddRangeAsync(BuildBrands(), cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!hasFeatures)
        {
            await _dbContext.Features.AddRangeAsync(BuildFeatures(), cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureDefaultShowcasesAsync(cancellationToken);
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

    private static List<Brand> BuildBrands()
    {
        return new List<Brand>
        {
            new() { Name = "VitaCure", Slug = "vitacure", Description = "Platform icindeki temel private label marka kaydi.", IsActive = true },
            new() { Name = "Solgar", Slug = "solgar", Description = "Vitamin ve mineral urunleri icin referans marka.", IsActive = true },
            new() { Name = "Nature's Supreme", Slug = "natures-supreme", Description = "Takviye ve wellness kataloglarinda kullanilan marka grubu.", IsActive = true },
            new() { Name = "Ocean", Slug = "ocean", Description = "Omega ve cocuk destek urunleri icin hazir katalog markasi.", IsActive = true }
        };
    }

    private static List<Feature> BuildFeatures()
    {
        return new List<Feature>
        {
            new() { Name = "Urun Formu", Slug = "urun-formu", GroupName = "Form", OptionsContent = string.Join(Environment.NewLine, new[] { "Kapsul", "Tablet", "Sase", "Damla" }), IsActive = true },
            new() { Name = "Hedef Destek", Slug = "hedef-destek", GroupName = "Hedef", OptionsContent = string.Join(Environment.NewLine, new[] { "Uyku", "Enerji", "Bagisiklik", "Sindirim" }), IsActive = true },
            new() { Name = "Icerik Tipi", Slug = "icerik-tipi", GroupName = "Icerik", OptionsContent = string.Join(Environment.NewLine, new[] { "Vitamin", "Mineral", "Bitkisel", "Probiyotik" }), IsActive = true }
        };
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

    private async Task EnsureDefaultShowcasesAsync(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories
            .Include(x => x.Products)
            .Where(x => x.IsActive && x.Slug != "uncategorized")
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            return;
        }

        var categoryLookup = categories.ToDictionary(x => x.Slug, StringComparer.OrdinalIgnoreCase);
        var existingShowcases = await _dbContext.Showcases
            .Include(x => x.ShowcaseCategories)
            .Include(x => x.FeaturedProducts)
            .ToListAsync(cancellationToken);
        var definitions = BuildDefaultShowcaseDefinitions();
        var hasChanges = false;

        for (var index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            if (!categoryLookup.TryGetValue(definition.CategorySlug, out var category))
            {
                continue;
            }

            var featuredProductIds = category.Products
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Rating)
                .ThenBy(x => x.Name)
                .Take(7)
                .Select(x => x.Id)
                .ToArray();

            var showcase = existingShowcases.FirstOrDefault(x =>
                string.Equals(x.Name, definition.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Slug, definition.LegacySlug, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Slug, definition.Slug, StringComparison.OrdinalIgnoreCase));

            if (showcase is null)
            {
                showcase = new Showcase
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = definition.Name,
                    Slug = definition.Slug,
                    IconClass = definition.IconClass,
                    Title = category.Name,
                    Description = category.Description,
                    TagsContent = BuildDefaultTags(category.Slug),
                    BackgroundImageUrl = ResolveShowcaseBackgroundImage(category.Name, definition.CategorySlug),
                    IsDark = !string.Equals(definition.CategorySlug, "uyku-sagligi", StringComparison.OrdinalIgnoreCase),
                    SeoTitle = category.SeoTitle,
                    MetaDescription = category.MetaDescription,
                    IsActive = true,
                    ShowOnHome = true,
                    SortOrder = index + 1,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Showcases.Add(showcase);
                existingShowcases.Add(showcase);
                SyncShowcaseCategories(showcase, category.Id);
                SyncFeaturedProducts(showcase, featuredProductIds);
                hasChanges = true;
            }
            else
            {
                var repaired = RepairExistingShowcase(showcase, definition, category, featuredProductIds, index + 1);
                hasChanges = hasChanges || repaired;
            }
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static void SyncShowcaseCategories(Showcase showcase, int categoryId)
    {
        showcase.ShowcaseCategories.Clear();
        showcase.ShowcaseCategories.Add(new ShowcaseCategory
        {
            ShowcaseId = showcase.Id,
            CategoryId = categoryId
        });
    }

    private static void SyncFeaturedProducts(Showcase showcase, IReadOnlyList<int> productIds)
    {
        showcase.FeaturedProducts.Clear();
        foreach (var item in productIds.Distinct().Take(7).Select((productId, index) => new { productId, index }))
        {
            showcase.FeaturedProducts.Add(new ShowcaseFeaturedProduct
            {
                ShowcaseId = showcase.Id,
                ProductId = item.productId,
                SortOrder = item.index
            });
        }
    }

    private bool RepairExistingShowcase(
        Showcase showcase,
        DefaultShowcaseDefinition definition,
        Category category,
        IReadOnlyList<int> featuredProductIds,
        int sortOrder)
    {
        var hasChanges = false;

        if (string.IsNullOrWhiteSpace(showcase.Name))
        {
            showcase.Name = definition.Name;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.Slug))
        {
            showcase.Slug = definition.Slug;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.IconClass))
        {
            showcase.IconClass = definition.IconClass;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.Title))
        {
            showcase.Title = category.Name;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.Description))
        {
            showcase.Description = category.Description;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.TagsContent))
        {
            showcase.TagsContent = BuildDefaultTags(category.Slug);
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.BackgroundImageUrl))
        {
            showcase.BackgroundImageUrl = ResolveShowcaseBackgroundImage(category.Name, definition.CategorySlug);
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.SeoTitle))
        {
            showcase.SeoTitle = category.SeoTitle;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(showcase.MetaDescription))
        {
            showcase.MetaDescription = category.MetaDescription;
            hasChanges = true;
        }

        if (showcase.SortOrder <= 0)
        {
            showcase.SortOrder = sortOrder;
            hasChanges = true;
        }

        if (showcase.ShowcaseCategories.Count == 0)
        {
            SyncShowcaseCategories(showcase, category.Id);
            hasChanges = true;
        }

        if (showcase.FeaturedProducts.Count == 0 && featuredProductIds.Count > 0)
        {
            SyncFeaturedProducts(showcase, featuredProductIds);
            hasChanges = true;
        }

        if (hasChanges)
        {
            showcase.UpdatedAt = DateTime.UtcNow;
        }

        return hasChanges;
    }

    private static IReadOnlyList<DefaultShowcaseDefinition> BuildDefaultShowcaseDefinitions()
    {
        return new[]
        {
            new DefaultShowcaseDefinition("Uyku Sağlığı", "uyku-rutini", "uyku-sagligi", "uyku-sagligi", "fa-solid fa-moon"),
            new DefaultShowcaseDefinition("Multivitamin & Enerji", "multivitamin-enerji-plani", "multivitamin-enerji", "multivitamin-enerji", "fa-solid fa-sun"),
            new DefaultShowcaseDefinition("Zihin & Hafıza Güçlendirme", "zihin-hafiza-rotasi", "zihin-hafiza-guclendirme", "zihin-hafiza-guclendirme", "fa-solid fa-brain"),
            new DefaultShowcaseDefinition("Hastalıklara Karşı Koruma", "bagisiklik-koruma-plani", "hastaliklara-karsi-koruma", "hastaliklara-karsi-koruma", "fa-solid fa-shield-heart"),
            new DefaultShowcaseDefinition("Kas ve İskelet Sağlığı", "kas-iskelet-destegi", "kas-ve-iskelet-sagligi", "kas-ve-iskelet-sagligi", "fa-solid fa-bone"),
            new DefaultShowcaseDefinition("Zayıflama Desteği", "zayiflama-rotasi", "zayiflama-destegi", "zayiflama-destegi", "fa-solid fa-person-running")
        };
    }

    private static string BuildDefaultTags(string slug)
    {
        return slug switch
        {
            "uyku-sagligi" => string.Join(Environment.NewLine, new[] { "Melatonin", "Gece Rutini", "Rahatlama" }),
            "multivitamin-enerji" => string.Join(Environment.NewLine, new[] { "Enerji", "Günlük Destek", "B12" }),
            "zihin-hafiza-guclendirme" => string.Join(Environment.NewLine, new[] { "Odak", "Hafıza", "Zihinsel Performans" }),
            "hastaliklara-karsi-koruma" => string.Join(Environment.NewLine, new[] { "Bağışıklık", "Koruma", "C Vitamini" }),
            "kas-ve-iskelet-sagligi" => string.Join(Environment.NewLine, new[] { "Kemik", "Eklem", "Kas Desteği" }),
            "zayiflama-destegi" => string.Join(Environment.NewLine, new[] { "Metabolizma", "Yağ Yakımı", "Diyet Desteği" }),
            _ => string.Empty
        };
    }

    private string ResolveShowcaseBackgroundImage(string? name, string? slug)
    {
        var imageRoot = Path.Combine(_environment.WebRootPath, "img");
        if (!Directory.Exists(imageRoot))
        {
            return string.Empty;
        }

        var explicitMatch = GetExplicitBackgroundImage(slug);
        if (!string.IsNullOrWhiteSpace(explicitMatch))
        {
            return explicitMatch;
        }

        var candidates = GetBackgroundAssetPaths(imageRoot)
            .Select(path => new
            {
                Path = $"/img/{Path.GetFileName(path)}",
                Normalized = NormalizeForMatch(Path.GetFileNameWithoutExtension(path))
            })
            .ToList();

        var searchTerms = new[]
        {
            NormalizeForMatch(name),
            NormalizeForMatch(slug)
        }.Where(term => !string.IsNullOrWhiteSpace(term)).ToArray();

        foreach (var term in searchTerms)
        {
            var match = candidates.FirstOrDefault(item =>
                item.Normalized.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                term.Contains(item.Normalized, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match.Path;
            }
        }

        return candidates.FirstOrDefault()?.Path ?? string.Empty;
    }

    private static string GetExplicitBackgroundImage(string? slug)
    {
        return slug?.Trim().ToLowerInvariant() switch
        {
            "uyku-sagligi" or "uyku-rutini" => "/img/uykuBg.png",
            "multivitamin-enerji" or "multivitamin-enerji-plani" => "/img/multivitaminBg.png",
            "zihin-hafiza-guclendirme" or "zihin-hafiza-rotasi" => "/img/zekaHafızaBg.png",
            "hastaliklara-karsi-koruma" or "bagisiklik-koruma-plani" => "/img/hastalıkKorumaBg.png",
            "kas-ve-iskelet-sagligi" or "kas-iskelet-destegi" => "/img/kasİskeletBg.png",
            "zayiflama-destegi" or "zayiflama-rotasi" => "/img/zayıflamaBg.png",
            _ => string.Empty
        };
    }

    private static IReadOnlyList<string> GetBackgroundAssetPaths(string imageRoot)
    {
        return Directory.GetFiles(imageRoot, "*Bg.png", SearchOption.TopDirectoryOnly)
            .Where(path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                return fileName.EndsWith("Bg", StringComparison.OrdinalIgnoreCase)
                    && !fileName.Contains("nobg", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
    }

    private static string NormalizeForMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("İ", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("&", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("destegi", string.Empty)
            .Replace("sagligi", string.Empty)
            .Replace("guclendirme", string.Empty)
            .Replace("karsi", string.Empty)
            .Replace("rotasi", string.Empty)
            .Replace("plani", string.Empty);
    }

    private sealed record DefaultShowcaseDefinition(
        string Name,
        string Slug,
        string LegacySlug,
        string CategorySlug,
        string IconClass);

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
