namespace vitacure.Application.Abstractions;

public interface ISlugService
{
    Task EnsureAvailableAsync(
        string slug,
        SlugEntityType entityType,
        int? currentEntityId = null,
        CancellationToken cancellationToken = default);

    Task<StorefrontSlugMatch> ResolveStorefrontAsync(string slug, CancellationToken cancellationToken = default);
}

public enum SlugEntityType
{
    Category = 1,
    Product = 2,
    Tag = 3,
    Showcase = 4
}

public enum StorefrontSlugTargetType
{
    None = 0,
    Category = 1,
    Product = 2,
    Showcase = 3
}

public sealed record StorefrontSlugMatch(StorefrontSlugTargetType TargetType, string Slug);
