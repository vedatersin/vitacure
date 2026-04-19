using System.Text.Json;
using System.Text.Json.Serialization;
using vitacure.Domain.Entities;

namespace vitacure.Infrastructure.Services;

internal static class ProductMediaSync
{
    public static IReadOnlyList<NormalizedProductMediaItem> Normalize(string? mediaItemsJson, string? coverImageUrl, string? galleryImageUrls)
    {
        var itemsFromJson = ParseJson(mediaItemsJson);
        if (itemsFromJson.Count > 0)
        {
            return itemsFromJson
                .Where(item => !string.IsNullOrWhiteSpace(item.Url))
                .Select((item, index) => new NormalizedProductMediaItem(
                    item.Url!.Trim(),
                    item.AssetId,
                    index,
                    index == 0))
                .DistinctBy(item => item.Url, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        return NormalizeLegacy(coverImageUrl, galleryImageUrls);
    }

    public static IReadOnlyList<NormalizedProductMediaItem> NormalizeLegacy(string? coverImageUrl, string? galleryImageUrls)
    {
        var urls = new[] { coverImageUrl }
            .Concat(SplitUrls(galleryImageUrls))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return urls
            .Select((url, index) => new NormalizedProductMediaItem(
                url,
                null,
                index,
                index == 0))
            .ToArray();
    }

    public static void SyncLegacyFields(Product product)
    {
        var orderedMedia = product.ProductMedias
            .OrderByDescending(item => item.IsPrimary)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        product.ImageUrl = orderedMedia.FirstOrDefault()?.Url ?? string.Empty;
        product.GalleryImageUrls = orderedMedia.Count <= 1
            ? null
            : string.Join(Environment.NewLine, orderedMedia.Skip(1).Select(item => item.Url));
    }

    public static IReadOnlyList<string> GetOrderedUrls(Product product)
    {
        var mediaUrls = product.ProductMedias
            .OrderByDescending(item => item.IsPrimary)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => item.Url)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (mediaUrls.Length > 0)
        {
            return mediaUrls;
        }

        return NormalizeLegacy(product.ImageUrl, product.GalleryImageUrls)
            .Select(item => item.Url)
            .ToArray();
    }

    private static IReadOnlyList<ProductMediaJsonItem> ParseJson(string? mediaItemsJson)
    {
        if (string.IsNullOrWhiteSpace(mediaItemsJson))
        {
            return Array.Empty<ProductMediaJsonItem>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<ProductMediaJsonItem>>(mediaItemsJson);
            return items ?? new List<ProductMediaJsonItem>();
        }
        catch
        {
            return Array.Empty<ProductMediaJsonItem>();
        }
    }

    private static IEnumerable<string> SplitUrls(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Array.Empty<string>();
        }

        return rawValue
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value));
    }
}

internal sealed record NormalizedProductMediaItem(string Url, int? AssetId, int SortOrder, bool IsPrimary);

internal sealed class ProductMediaJsonItem
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("assetId")]
    public int? AssetId { get; set; }
}
