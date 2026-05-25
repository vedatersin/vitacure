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
        await EnsureDefaultGoogleProductCategoriesAsync(cancellationToken);
        await EnsureDefaultCustomFieldDefinitionsAsync(cancellationToken);
        await EnsureDefaultPersonalizationDefinitionsAsync(cancellationToken);
        await EnsureDefaultShowcasesAsync(cancellationToken);
    }

    private async Task EnsureDefaultGoogleProductCategoriesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.GoogleProductCategories.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var definitions = new[]
        {
            "Bavullar ve Cantalar",
            "Bebek ve Kucuk Cocuk ?r?nleri",
            "Buro Malzemeleri",
            "Din ve Torenler",
            "Elektronik",
            "Ev ve Bahce",
            "Hayvanlar ve Evcil Hayvan ?r?nleri",
            "Hirdavat",
            "Kameralar ve Optik Malzemeler",
            "Kiyafet ve Aksesuarlar",
            "Medya",
            "Mobilyalar",
            "Oyuncaklar ve Oyunlar",
            "Sanat ve Eglence",
            "Saglik ve Guzellik",
            "Spor Malzemeleri",
            "Tasitlar ve Parcalar",
            "Yazilim",
            "Yetiskinlere Yonelik ?r?nler",
            "Yiyecek, Icecekler ve Tutun Mamulleri",
            "Is ve Endustri"
        };

        var categories = definitions
            .Select((name, index) => new GoogleProductCategory
            {
                Name = name,
                Slug = Slugify(name),
                IsActive = true,
                SortOrder = index + 1
            })
            .ToList();

        _dbContext.GoogleProductCategories.AddRange(categories);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultCustomFieldDefinitionsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.CustomFieldDefinitions.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.CustomFieldDefinitions.AddRange(
            new CustomFieldDefinition { Name = "Yikama Talimatlari", Slug = "yikama-talimatlari", FieldType = "HTML", IsFilterable = false, IsActive = true },
            new CustomFieldDefinition { Name = "Teknik ?zellikler", Slug = "teknik-ozellikler", FieldType = "Table", IsFilterable = true, IsActive = true },
            new CustomFieldDefinition { Name = "?l?? Tablosu", Slug = "olcu-tablosu", FieldType = "Table", IsFilterable = false, IsActive = true });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultPersonalizationDefinitionsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.PersonalizationDefinitions.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.PersonalizationDefinitions.AddRange(
            new PersonalizationDefinition { Name = "Hediye Notu", Slug = "hediye-notu", InputType = "Text", IsActive = true },
            new PersonalizationDefinition { Name = "Dosya Yukleme", Slug = "dosya-yukleme", InputType = "File", IsActive = true },
            new PersonalizationDefinition { Name = "Tarih", Slug = "tarih", InputType = "Date", IsActive = true });

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
            new() { Name = "?r?n Formu", Slug = "urun-formu", GroupName = "Form", OptionsContent = string.Join(Environment.NewLine, new[] { "Kapsul", "Tablet", "Sase", "Damla" }), IsActive = true },
            new() { Name = "Hedef Destek", Slug = "hedef-destek", GroupName = "Hedef", OptionsContent = string.Join(Environment.NewLine, new[] { "Uyku", "Enerji", "Bagisiklik", "Sindirim" }), IsActive = true },
            new() { Name = "I?erik Tipi", Slug = "icerik-tipi", GroupName = "I?erik", OptionsContent = string.Join(Environment.NewLine, new[] { "Vitamin", "Mineral", "Bitkisel", "Probiyotik" }), IsActive = true }
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
            .Replace("�", "c")
            .Replace("g", "g")
            .Replace("i", "i")
            .Replace("�", "o")
            .Replace("s", "s")
            .Replace("�", "u")
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
            .Include(x => x.Prompts)
            .Include(x => x.Tags)
            .ToListAsync(cancellationToken);
        var definitions = BuildDefaultShowcaseDefinitions();
        var hasChanges = false;
        var usedCategoryIds = new HashSet<int>();

        for (var index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            if (!TryResolveShowcaseCategory(categoryLookup, categories, definition, usedCategoryIds, out var category))
            {
                continue;
            }

            usedCategoryIds.Add(category.Id);

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
                    IconColor = GetDefaultShowcaseIconColor(definition.CategorySlug),
                    Title = category.Name,
                    Description = category.Description,
                    TagsContent = BuildDefaultTags(category.Slug),
                    ExamplePromptsContent = string.Join(Environment.NewLine, GetDefaultShowcasePrompts(definition.CategorySlug)),
                    BackgroundImageUrl = ResolveShowcaseBackgroundImage(category.Name, definition.CategorySlug),
                    PrimaryCategoryId = category.Id,
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
                SyncShowcaseTags(showcase, category.Slug);
                SyncShowcasePrompts(showcase, definition.CategorySlug);
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

    private static void SyncShowcasePrompts(Showcase showcase, string categorySlug)
    {
        showcase.Prompts.Clear();
        foreach (var item in GetDefaultShowcasePrompts(categorySlug).Select((text, index) => new { text, index }))
        {
            showcase.Prompts.Add(new ShowcasePrompt
            {
                ShowcaseId = showcase.Id,
                Text = item.text,
                SortOrder = item.index
            });
        }
    }

    private static void SyncShowcaseTags(Showcase showcase, string categorySlug)
    {
        showcase.Tags.Clear();
        foreach (var item in BuildDefaultTags(categorySlug)
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((text, index) => new { text, index }))
        {
            showcase.Tags.Add(new ShowcaseTag
            {
                ShowcaseId = showcase.Id,
                Name = item.text,
                Slug = Slugify(item.text),
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

        if (string.IsNullOrWhiteSpace(showcase.IconColor))
        {
            showcase.IconColor = GetDefaultShowcaseIconColor(definition.CategorySlug);
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

        if (string.IsNullOrWhiteSpace(showcase.ExamplePromptsContent))
        {
            showcase.ExamplePromptsContent = string.Join(Environment.NewLine, GetDefaultShowcasePrompts(definition.CategorySlug));
            hasChanges = true;
        }

        if (!showcase.PrimaryCategoryId.HasValue)
        {
            showcase.PrimaryCategoryId = category.Id;
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

        if (showcase.Prompts.Count == 0)
        {
            SyncShowcasePrompts(showcase, definition.CategorySlug);
            hasChanges = true;
        }

        if (showcase.Tags.Count == 0)
        {
            SyncShowcaseTags(showcase, category.Slug);
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
            new DefaultShowcaseDefinition("Uyku Sagligi", "uyku-rutini", "uyku-sagligi", "uyku-sagligi", "fa-solid fa-moon"),
            new DefaultShowcaseDefinition("Multivitamin & Enerji", "multivitamin-enerji-plani", "multivitamin-enerji", "multivitamin-enerji", "fa-solid fa-sun"),
            new DefaultShowcaseDefinition("Zihin & Hafiza G��lendirme", "zihin-hafiza-rotasi", "zihin-hafiza-guclendirme", "zihin-hafiza-guclendirme", "fa-solid fa-brain"),
            new DefaultShowcaseDefinition("Hastaliklara Karsi Koruma", "bagisiklik-koruma-plani", "hastaliklara-karsi-koruma", "hastaliklara-karsi-koruma", "fa-solid fa-shield-heart"),
            new DefaultShowcaseDefinition("Kas ve Iskelet Sagligi", "kas-iskelet-destegi", "kas-ve-iskelet-sagligi", "kas-ve-iskelet-sagligi", "fa-solid fa-bone"),
            new DefaultShowcaseDefinition("Zayiflama Destegi", "zayiflama-rotasi", "zayiflama-destegi", "zayiflama-destegi", "fa-solid fa-person-running")
        };
    }

    private static bool TryResolveShowcaseCategory(
        IReadOnlyDictionary<string, Category> categoryLookup,
        IReadOnlyList<Category> categories,
        DefaultShowcaseDefinition definition,
        IReadOnlySet<int> usedCategoryIds,
        out Category category)
    {
        if (categoryLookup.TryGetValue(definition.CategorySlug, out category!))
        {
            return true;
        }

        var matchTerms = GetMatchTerms(definition);
        category = categories
            .Where(item => !usedCategoryIds.Contains(item.Id))
            .Select(item => new
            {
                Category = item,
                Score = matchTerms.Count(term =>
                    NormalizeForMatch(item.Name).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    NormalizeForMatch(item.Slug).Contains(term, StringComparison.OrdinalIgnoreCase))
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Category.Name.Length)
            .Select(item => item.Category)
            .FirstOrDefault()!;

        return category is not null;
    }

    private static IReadOnlyList<string> GetMatchTerms(DefaultShowcaseDefinition definition)
    {
        return definition.Slug switch
        {
            "uyku-rutini" => ["uyku", "melatonin"],
            "multivitamin-enerji-plani" => ["multivitamin", "enerji", "b12"],
            "zihin-hafiza-rotasi" => ["zihin", "hafiza", "odak", "omega"],
            "bagisiklik-koruma-plani" => ["bagisiklik", "koruma", "beta glukan", "c vitamini"],
            "kas-iskelet-destegi" => ["kas", "kemik", "eklem", "kolajen"],
            "zayiflama-rotasi" => ["zayiflama", "metabolizma", "odem", "detoks"],
            _ => Array.Empty<string>()
        };
    }

    private static string BuildDefaultTags(string slug)
    {
        return slug switch
        {
            "uyku-sagligi" => string.Join(Environment.NewLine, new[] { "Melatonin", "Gece Rutini", "Rahatlama" }),
            "multivitamin-enerji" => string.Join(Environment.NewLine, new[] { "Enerji", "G�nl�k Destek", "B12" }),
            "zihin-hafiza-guclendirme" => string.Join(Environment.NewLine, new[] { "Odak", "Hafiza", "Zihinsel Performans" }),
            "hastaliklara-karsi-koruma" => string.Join(Environment.NewLine, new[] { "Bagisiklik", "Koruma", "C Vitamini" }),
            "kas-ve-iskelet-sagligi" => string.Join(Environment.NewLine, new[] { "Kemik", "Eklem", "Kas Destegi" }),
            "zayiflama-destegi" => string.Join(Environment.NewLine, new[] { "Metabolizma", "Yag Yakimi", "Diyet Destegi" }),
            _ => string.Empty
        };
    }

    private static string GetDefaultShowcaseIconColor(string slug)
    {
        return slug switch
        {
            "uyku-sagligi" => "#4b63d3",
            "multivitamin-enerji" => "#d6a11d",
            "zihin-hafiza-guclendirme" => "#d4569a",
            "hastaliklara-karsi-koruma" => "#35a966",
            "kas-ve-iskelet-sagligi" => "#d94b57",
            "zayiflama-destegi" => "#e07a2f",
            _ => "#4b63d3"
        };
    }

    private static IReadOnlyList<string> GetDefaultShowcasePrompts(string slug)
    {
        return slug switch
        {
            "uyku-sagligi" => [
                "Gece rutini icin hangi destekleri onerirsin?",
                "Uykuya dalmakta zorlanirsam hangi urunlere bakmaliyim?",
                "Daha derin ve kaliteli uyku icin bana bir kombin hazirla."
            ],
            "multivitamin-enerji" => [
                "Gun boyu enerjimi destekleyecek bir rutin kurabilir misin?",
                "Yorgunluk icin multivitamin ve B12 tarafinda ne onerirsin?",
                "Sabah daha dinamik hissetmek icin bana urun sec."
            ],
            "zihin-hafiza-guclendirme" => [
                "Odaklanma ve hafiza icin hangi destekler uygun olur?",
                "Zihinsel performansi destekleyen urunleri gosterir misin?",
                "Calisirken konsantrasyonumu artiracak bir kombin isterim."
            ],
            "hastaliklara-karsi-koruma" => [
                "Bagisiklik destegi icin nereden baslamaliyim?",
                "Mevsim gecislerinde koruyucu bir rutin onerir misin?",
                "Hastaliga karsi gunluk destek urunlerini listele."
            ],
            "kas-ve-iskelet-sagligi" => [
                "Eklem ve kemik destegi icin hangi urunler uygun?",
                "Spor sonrasi kas toparlanmasi icin ne onerirsin?",
                "Kas ve iskelet sagligi icin bana bir paket cikar."
            ],
            "zayiflama-destegi" => [
                "Kilo kontrolu icin destek urunleri gosterir misin?",
                "Metabolizma ve odem tarafinda ne onerirsin?",
                "Zayiflama hedefim icin bana bir baslangic rutini hazirla."
            ],
            _ => Array.Empty<string>()
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
            "zihin-hafiza-guclendirme" or "zihin-hafiza-rotasi" => "/img/zekaHafizaBg.png",
            "hastaliklara-karsi-koruma" or "bagisiklik-koruma-plani" => "/img/hastalikKorumaBg.png",
            "kas-ve-iskelet-sagligi" or "kas-iskelet-destegi" => "/img/kasIskeletBg.png",
            "zayiflama-destegi" or "zayiflama-rotasi" => "/img/zayiflamaBg.png",
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
            .Replace("i", "i")
            .Replace("I", "i")
            .Replace("g", "g")
            .Replace("�", "u")
            .Replace("s", "s")
            .Replace("�", "o")
            .Replace("�", "c")
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
