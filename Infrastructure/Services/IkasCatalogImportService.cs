using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;

namespace vitacure.Infrastructure.Services;

public class IkasCatalogImportService
{
    private const string UncategorizedCategorySlug = "diger-urunler";
    private const string UncategorizedCategoryName = "DiÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€šÃ‚Â¸er ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œrÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼nler";

    private static readonly IReadOnlyList<ImportFeatureDefinition> FeatureDefinitions =
    [
        new("urun-tipi", "ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œrÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼n Tipi", "Ikas Teknik", row => row.Type),
        new("tedarikci", "TedarikÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â§i", "Ikas Teknik", row => row.Supplier),
        new("google-urun-kategorisi", "Google ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œrÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼n Kategorisi", "Ikas Teknik", row => row.GoogleProductCategory),
        new("barkod", "Barkod", "Ikas Teknik", row => row.BarcodeList),
        new("sku", "SKU", "Ikas Teknik", row => row.Sku),
        new("alis-fiyati", "Alis Fiyati", "Ikas Teknik", row => row.PurchasePrice),
        new("desi", "Desi", "Ikas Teknik", row => row.Desi),
        new("hs-kodu", "HS Kodu", "Ikas Teknik", row => row.HsCode),
        new("birim-urun-miktari", "Birim ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œrÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼n MiktarÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±", "Ikas Teknik", row => CombineValueAndUnit(row.UnitProductQuantity, row.ProductUnit)),
        new("satilan-urun-miktari", "SatÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±lan ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œrÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼n MiktarÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±", "Ikas Teknik", row => CombineValueAndUnit(row.SoldProductQuantity, row.SoldProductUnit)),
        new("stok-tukenince-satis", "Stok TÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼kenince SatÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¦ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€šÃ‚Â¸", "Ikas Teknik", row => row.ContinueSellingWhenOutOfStock),
        new("satis-kanali", "SatÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¦ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€šÃ‚Â¸ KanalÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±", "Ikas Teknik", row => row.SalesChannel),
        new("sepet-minimum-adet", "Sepet Minimum Adet", "Ikas Teknik", row => row.MinBasketQuantity),
        new("sepet-maksimum-adet", "Sepet Maksimum Adet", "Ikas Teknik", row => row.MaxBasketQuantity),
        new("ikas-urun-grup-id", "ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â°kas ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œrÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼n Grup ID", "Ikas Referans", row => row.ProductGroupId),
        new("ikas-varyant-id", "Ikas Varyant ID", "Ikas Referans", row => row.VariantId),
        new("ikas-olusturulma-tarihi", "Ikas Olusturulma Tarihi", "Ikas Referans", row => row.CreatedAt)
    ];

    private readonly AppDbContext _dbContext;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbSeeder _dbSeeder;

    public IkasCatalogImportService(
        AppDbContext dbContext,
        ICacheInvalidationService cacheInvalidationService,
        AppDbSeeder dbSeeder)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _dbSeeder = dbSeeder;
    }

    public async Task ImportFromJsonAsync(string jsonPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath))
        {
            throw new FileNotFoundException("Ikas import JSON dosyasi bulunamadi.", jsonPath);
        }

        await using var stream = File.OpenRead(jsonPath);
        var rows = await JsonSerializer.DeserializeAsync<List<IkasImportRow>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken) ?? [];

        var activeRows = rows
            .Where(row => !row.IsDeleted && !string.IsNullOrWhiteSpace(row.Name))
            .ToList();

        if (activeRows.Count == 0)
        {
            throw new InvalidOperationException("Aktarilacak aktif urun satiri bulunamadi.");
        }

        await ClearCatalogAsync(cancellationToken);

        var usedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var categories = BuildCategories(activeRows, usedSlugs);
        var brands = BuildBrands(activeRows, usedSlugs);
        var tags = BuildTags(activeRows, usedSlugs);
        var features = BuildFeatures(activeRows, usedSlugs);

        await _dbContext.Categories.AddRangeAsync(categories, cancellationToken);
        await _dbContext.Brands.AddRangeAsync(brands, cancellationToken);
        await _dbContext.Tags.AddRangeAsync(tags, cancellationToken);
        await _dbContext.Features.AddRangeAsync(features, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var categoryMap = await _dbContext.Categories.AsNoTracking().ToDictionaryAsync(x => x.Slug, x => x, cancellationToken);
        var brandMap = await _dbContext.Brands.AsNoTracking().ToDictionaryAsync(x => x.Slug, x => x, cancellationToken);
        var tagMap = await _dbContext.Tags.AsNoTracking().ToDictionaryAsync(x => x.Slug, x => x, cancellationToken);
        var featureMap = await _dbContext.Features.AsNoTracking().ToDictionaryAsync(x => x.Slug, x => x, cancellationToken);

        var productSlugSet = new HashSet<string>(usedSlugs, StringComparer.OrdinalIgnoreCase);
        var products = new List<Product>();
        var pendingProductFeatures = new List<(Product Product, int FeatureId, string Value)>();
        foreach (var row in activeRows)
        {
            var categorySlugs = SplitMultiValue(row.Categories)
                .Select(value => Slugify(value))
                .Where(value => categoryMap.ContainsKey(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (categorySlugs.Count == 0)
            {
                categorySlugs.Add(UncategorizedCategorySlug);
            }

            var primaryCategory = categoryMap[categorySlugs[0]];
            var normalizedBrandSlug = Slugify(row.Brand);
            Brand? brand = null;
            if (!string.IsNullOrWhiteSpace(normalizedBrandSlug))
            {
                brandMap.TryGetValue(normalizedBrandSlug, out brand);
            }

            int? brandId = brand?.Id;

            var name = NormalizeProductName(row.Name);
            var productSlug = CreateUniqueSlug(
                string.IsNullOrWhiteSpace(row.Slug) ? name : row.Slug,
                productSlugSet,
                "urun");

            var description = BuildDescription(row, name, primaryCategory.Name, brand?.Name);
            var metaTitle = NormalizeOptional(row.MetadataTitle) ?? name;
            var metaDescription = BuildMetaDescription(row, description, primaryCategory.Name, brand?.Name);
            var currentPrice = ResolveCurrentPrice(row.SalePrice, row.DiscountedPrice);
            var oldPrice = ResolveOldPrice(row.SalePrice, row.DiscountedPrice);
            var stock = ParseInt(row.Stock);
            var primaryImageUrl = ResolvePrimaryImageUrl(row.ImageUrl);

            var product = new Product
            {
                Name = name,
                Slug = productSlug,
                Description = description,
                MetaTitle = metaTitle,
                MetaDescription = metaDescription,
                Price = currentPrice,
                OldPrice = oldPrice,
                Rating = 5m,
                ImageUrl = primaryImageUrl,
                BrandId = brandId,
                CategoryId = primaryCategory.Id,
                Stock = stock,
                IsActive = IsVisible(row),
                CreatedAt = ParseDateTime(row.CreatedAt) ?? DateTime.UtcNow
            };

            foreach (var categorySlug in categorySlugs)
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = categoryMap[categorySlug].Id
                });
            }

            foreach (var tagValue in SplitMultiValue(row.Tags))
            {
                var tagSlug = Slugify(tagValue);
                if (!tagMap.TryGetValue(tagSlug, out var tag))
                {
                    continue;
                }

                product.ProductTags.Add(new ProductTag
                {
                    TagId = tag.Id
                });
            }

            foreach (var featureDefinition in FeatureDefinitions)
            {
                var value = NormalizeOptional(featureDefinition.ValueSelector(row));
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (!featureMap.TryGetValue(GetFeatureSlug(featureDefinition), out var feature))
                {
                    continue;
                }

                pendingProductFeatures.Add((product, feature.Id, value));
            }

            product.ProductMedias.Add(new ProductMedia
            {
                Url = product.ImageUrl,
                AltText = product.Name,
                SortOrder = 0,
                IsPrimary = true,
                UpdatedAt = DateTime.UtcNow
            });

            products.Add(product);
        }

        await _dbContext.Products.AddRangeAsync(products, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (pendingProductFeatures.Count > 0)
        {
            await _dbContext.ProductFeatures.AddRangeAsync(
                pendingProductFeatures.Select(item => new ProductFeature
                {
                    ProductId = item.Product.Id,
                    FeatureId = item.FeatureId,
                    Value = item.Value
                }),
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await _dbSeeder.SeedAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateProductAsync(cancellationToken);
    }

    private async Task ClearCatalogAsync(CancellationToken cancellationToken)
    {
        await _dbContext.CustomerCartItems.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CustomerFavorites.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Database.ExecuteSqlRawAsync("UPDATE [OrderItems] SET [ProductVariantId] = NULL, [ProductId] = NULL", cancellationToken);
        await _dbContext.ShowcaseFeaturedProducts.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ShowcaseCategories.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Showcases.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductCollections.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductFeatures.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductTags.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductCategories.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductMedias.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductVariants.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Products.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Collections.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Tags.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Features.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Brands.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Categories.ExecuteDeleteAsync(cancellationToken);
    }

    private static List<Category> BuildCategories(IEnumerable<IkasImportRow> rows, HashSet<string> usedSlugs)
    {
        var groups = rows
            .SelectMany(row => SplitMultiValue(row.Categories))
            .GroupBy(value => Slugify(value), StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key));

        var categories = new List<Category>();
        foreach (var group in groups.OrderBy(group => group.Key))
        {
            var name = CanonicalizeDisplayName(group);
            categories.Add(new Category
            {
                Name = name,
                Slug = CreateUniqueSlug(group.Key, usedSlugs, "kategori"),
                Description = $"{name} kategorisindeki urunler ikas katalog aktarimi ile olusturuldu.",
                SeoTitle = $"{name} | VitaCure",
                MetaDescription = $"{name} kategorisindeki urunler VitaCure katalog sayfasinda listelenir.",
                IsActive = true
            });
        }

        if (rows.Any(row => !SplitMultiValue(row.Categories).Any()))
        {
            categories.Add(new Category
            {
                Name = UncategorizedCategoryName,
                Slug = CreateUniqueSlug(UncategorizedCategorySlug, usedSlugs, "kategori"),
                Description = "Kategori bilgisi gelmeyen urunler icin otomatik olusturulan kapsayici kategori.",
                SeoTitle = $"{UncategorizedCategoryName} | VitaCure",
                MetaDescription = "Kategori bilgisi eksik urunler VitaCure katalog aktarmasi sirasinda bu kategoride toplandi.",
                IsActive = true
            });
        }

        return categories;
    }

    private static List<Brand> BuildBrands(IEnumerable<IkasImportRow> rows, HashSet<string> usedSlugs)
    {
        var groups = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Brand))
            .GroupBy(row => Slugify(row.Brand), StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key));

        var brands = new List<Brand>();
        foreach (var group in groups.OrderBy(group => group.Key))
        {
            var name = CanonicalizeDisplayName(group.Select(item => item.Brand!).Where(item => !string.IsNullOrWhiteSpace(item)));
            brands.Add(new Brand
            {
                Name = name,
                Slug = CreateUniqueSlug(group.Key, usedSlugs, "marka"),
                Description = $"{name} markasi ikas urun aktarimi ile olusturuldu.",
                IsActive = true
            });
        }

        return brands;
    }

    private static List<Tag> BuildTags(IEnumerable<IkasImportRow> rows, HashSet<string> usedSlugs)
    {
        var groups = rows
            .SelectMany(row => SplitMultiValue(row.Tags))
            .GroupBy(value => Slugify(value), StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key));

        var tags = new List<Tag>();
        foreach (var group in groups.OrderBy(group => group.Key))
        {
            tags.Add(new Tag
            {
                Name = CanonicalizeDisplayName(group),
                Slug = CreateUniqueSlug(group.Key, usedSlugs, "etiket")
            });
        }

        return tags;
    }

    private static List<Feature> BuildFeatures(IEnumerable<IkasImportRow> rows, HashSet<string> usedSlugs)
    {
        var features = new List<Feature>();
        foreach (var definition in FeatureDefinitions)
        {
            var values = rows
                .Select(definition.ValueSelector)
                .Select(NormalizeOptional)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToArray();

            if (values.Length == 0)
            {
                continue;
            }

            features.Add(new Feature
            {
                Name = definition.Name,
                Slug = CreateUniqueSlug(GetFeatureSlug(definition), usedSlugs, "ozellik"),
                GroupName = definition.GroupName,
                OptionsContent = values.Length <= 40 ? string.Join(Environment.NewLine, values) : null,
                IsActive = true
            });
        }

        return features;
    }

    private static string BuildDescription(IkasImportRow row, string name, string primaryCategoryName, string? brandName)
    {
        var provided = NormalizeOptional(row.Description);
        if (!string.IsNullOrWhiteSpace(provided))
        {
            return provided!;
        }

        var allCategories = SplitMultiValue(row.Categories).ToArray();
        var categorySummary = allCategories.Length > 1
            ? $"{primaryCategoryName} odakli olup {string.Join(", ", allCategories.Skip(1))} ile de iliskilidir."
            : $"{primaryCategoryName} kategorisinde listelenmektedir.";
        var brandSummary = string.IsNullOrWhiteSpace(brandName)
            ? "Bu urun VitaCure katalog aktarimi ile sisteme eklenmistir."
            : $"{brandName} markasina ait bu urun VitaCure katalog aktarimi ile sisteme eklenmistir.";

        return $"<p>{name} icin urun aciklamasi kaynaktan gelmedigi icin teknik katalog verilerine gore olusturuldu.</p><p>{brandSummary} {categorySummary}</p>";
    }

    private static string BuildMetaDescription(IkasImportRow row, string description, string primaryCategoryName, string? brandName)
    {
        var provided = NormalizeOptional(row.MetadataDescription);
        if (!string.IsNullOrWhiteSpace(provided))
        {
            return provided!;
        }

        var source = string.IsNullOrWhiteSpace(brandName)
            ? $"{row.Name} - {primaryCategoryName}"
            : $"{row.Name} - {brandName} - {primaryCategoryName}";

        return Truncate($"{source}. {StripHtml(description)}", 500);
    }

    private static decimal ResolveCurrentPrice(string? salePrice, string? discountedPrice)
    {
        var sale = ParseDecimal(salePrice);
        var discounted = ParseNullableDecimal(discountedPrice);
        return discounted is > 0 ? discounted.Value : sale;
    }

    private static decimal? ResolveOldPrice(string? salePrice, string? discountedPrice)
    {
        var sale = ParseDecimal(salePrice);
        var discounted = ParseNullableDecimal(discountedPrice);
        return discounted is > 0 && sale > discounted.Value ? sale : null;
    }

    private static bool IsVisible(IkasImportRow row)
    {
        var channel = NormalizeOptional(row.SalesChannel);
        return !row.IsDeleted && !string.Equals(channel, "HIDDEN", StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var direct))
        {
            return direct;
        }

        var rounded = Math.Round(ParseDecimal(value), MidpointRounding.AwayFromZero);
        if (rounded > int.MaxValue)
        {
            return int.MaxValue;
        }

        if (rounded < int.MinValue)
        {
            return int.MinValue;
        }

        return (int)rounded;
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        var normalized = value.Trim().Replace(",", ".");
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parsed = ParseDecimal(value);
        return parsed == 0m ? null : parsed;
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed
            : null;
    }

    private static IEnumerable<string> SplitMultiValue(string? rawValue)
    {
        return (rawValue ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string CanonicalizeDisplayName(IEnumerable<string> values)
    {
        var candidates = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();

        if (candidates.Count == 0)
        {
            return string.Empty;
        }

        var preferred = candidates
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Length)
            .Select(group => group.First())
            .First();

        return CanonicalizeDisplayName(preferred);
    }

    private static string CanonicalizeDisplayName(IGrouping<string, string> group)
        => CanonicalizeDisplayName(group.AsEnumerable());

    private static string CanonicalizeDisplayName(string rawValue)
    {
        var normalized = NormalizeOptional(rawValue) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var textInfo = culture.TextInfo;
        return normalized == normalized.ToLower(culture)
            ? textInfo.ToTitleCase(normalized)
            : normalized;
    }

    private static string NormalizeProductName(string rawValue)
        => NormalizeOptional(rawValue) ?? "Adsiz ?r?n";

    private static string? NormalizeOptional(string? value)
    {
        var normalized = RepairMojibake(value?.Trim());
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string ResolvePrimaryImageUrl(string? rawValue)
    {
        var candidates = (rawValue ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeOptional)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();

        if (candidates.Length == 0)
        {
            return "/img/logo-mini.png";
        }

        var preferredImage = candidates.FirstOrDefault(value => IsImageAsset(value));
        return preferredImage ?? "/img/logo-mini.png";
    }

    private static bool IsImageAsset(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var path = uri.AbsolutePath;
        return path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".avif", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFeatureSlug(ImportFeatureDefinition definition)
        => $"teknik-{definition.Slug}";

    private static string CreateUniqueSlug(string? rawValue, HashSet<string> usedSlugs, string fallbackPrefix)
    {
        var baseSlug = Slugify(rawValue);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = fallbackPrefix;
        }

        var candidate = baseSlug;
        var suffix = 2;
        while (!usedSlugs.Add(candidate))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }

        return candidate;
    }

    private static string Slugify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var character in (RepairMojibake(value) ?? string.Empty).Trim().ToLowerInvariant())
        {
            builder.Append(character switch
            {
                'ç' => 'c',
                'ğ' => 'g',
                'ı' => 'i',
                'ö' => 'o',
                'ş' => 's',
                'ü' => 'u',
                '&' => '-',
                '/' => '-',
                '+' => '-',
                _ when char.IsLetterOrDigit(character) => character,
                _ => '-'
            });
        }

        var slug = builder.ToString();
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    private static string CombineValueAndUnit(string? value, string? unit)
    {
        var normalizedValue = NormalizeOptional(value);
        var normalizedUnit = NormalizeOptional(unit);
        return string.IsNullOrWhiteSpace(normalizedUnit)
            ? normalizedValue ?? string.Empty
            : string.IsNullOrWhiteSpace(normalizedValue)
                ? normalizedUnit
                : $"{normalizedValue} {normalizedUnit}";
    }

    private static string? RepairMojibake(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (!value.Contains('Ã') && !value.Contains('Ä') && !value.Contains('Å') && !value.Contains('â'))
        {
            return value;
        }

        try
        {
            return Encoding.UTF8.GetString(Encoding.Latin1.GetBytes(value));
        }
        catch (DecoderFallbackException)
        {
            return value;
        }
    }

    private static string StripHtml(string value)
    {
        var insideTag = false;
        var builder = new StringBuilder();

        foreach (var character in value)
        {
            if (character == '<')
            {
                insideTag = true;
                continue;
            }

            if (character == '>')
            {
                insideTag = false;
                continue;
            }

            if (!insideTag)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Replace(Environment.NewLine, " ").Trim();
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength].Trim();

    private sealed record ImportFeatureDefinition(
        string Slug,
        string Name,
        string GroupName,
        Func<IkasImportRow, string?> ValueSelector);

    private sealed class IkasImportRow
    {
        public string ProductGroupId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SalePrice { get; set; }
        public string? DiscountedPrice { get; set; }
        public string? PurchasePrice { get; set; }
        public string? BarcodeList { get; set; }
        public string? Sku { get; set; }
        public bool IsDeleted { get; set; }
        public string? Brand { get; set; }
        public string? Categories { get; set; }
        public string? Tags { get; set; }
        public string? ImageUrl { get; set; }
        public string? MetadataTitle { get; set; }
        public string? MetadataDescription { get; set; }
        public string? Slug { get; set; }
        public string? Stock { get; set; }
        public string? Type { get; set; }
        public string? VariantType1 { get; set; }
        public string? VariantValue1 { get; set; }
        public string? VariantType2 { get; set; }
        public string? VariantValue2 { get; set; }
        public string? Desi { get; set; }
        public string? HsCode { get; set; }
        public string? UnitProductQuantity { get; set; }
        public string? ProductUnit { get; set; }
        public string? SoldProductQuantity { get; set; }
        public string? SoldProductUnit { get; set; }
        public string? GoogleProductCategory { get; set; }
        public string? Supplier { get; set; }
        public string? ContinueSellingWhenOutOfStock { get; set; }
        public string? SalesChannel { get; set; }
        public string? MinBasketQuantity { get; set; }
        public string? MaxBasketQuantity { get; set; }
        public bool VariantIsActive { get; set; }
        public string? CreatedAt { get; set; }
    }
}
