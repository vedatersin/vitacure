using Microsoft.EntityFrameworkCore;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Infrastructure.Persistence;

namespace vitacure.Infrastructure.Services;

public class SlugService : ISlugService
{
    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "yonetim",
        "login",
        "register",
        "confirm-email",
        "forgot-password",
        "reset-password",
        "logout",
        "account",
        "cart",
        "home",
        "error"
    };

    private readonly AppDbContext _dbContext;

    public SlugService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureAvailableAsync(
        string slug,
        SlugEntityType entityType,
        int? currentEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            throw new SlugConflictException(slug, "Slug alani bos birakilamaz.");
        }

        if (ReservedSlugs.Contains(normalizedSlug))
        {
            throw new SlugConflictException(
                normalizedSlug,
                $"'{normalizedSlug}' sistem route'u icin ayrildi. Farkli bir slug girin.");
        }

        await EnsureEntityConflictFreeAsync(
            _dbContext.Categories.AsNoTracking(),
            normalizedSlug,
            SlugEntityType.Category,
            entityType == SlugEntityType.Category ? currentEntityId : null,
            x => x.Id,
            x => x.Name,
            x => x.Slug,
            cancellationToken);

        await EnsureEntityConflictFreeAsync(
            _dbContext.Products.AsNoTracking(),
            normalizedSlug,
            SlugEntityType.Product,
            entityType == SlugEntityType.Product ? currentEntityId : null,
            x => x.Id,
            x => x.Name,
            x => x.Slug,
            cancellationToken);

        await EnsureEntityConflictFreeAsync(
            _dbContext.Tags.AsNoTracking(),
            normalizedSlug,
            SlugEntityType.Tag,
            entityType == SlugEntityType.Tag ? currentEntityId : null,
            x => x.Id,
            x => x.Name,
            x => x.Slug,
            cancellationToken);

        await EnsureEntityConflictFreeAsync(
            _dbContext.Showcases.AsNoTracking(),
            normalizedSlug,
            SlugEntityType.Showcase,
            entityType == SlugEntityType.Showcase ? currentEntityId : null,
            x => x.Id,
            x => x.Name,
            x => x.Slug,
            cancellationToken);
    }

    public async Task<StorefrontSlugMatch> ResolveStorefrontAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new StorefrontSlugMatch(StorefrontSlugTargetType.None, string.Empty);
        }

        var hasProduct = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.IsActive && x.Slug.ToLower() == normalizedSlug, cancellationToken);

        if (hasProduct)
        {
            return new StorefrontSlugMatch(StorefrontSlugTargetType.Product, normalizedSlug);
        }

        var hasShowcase = await _dbContext.Showcases
            .AsNoTracking()
            .AnyAsync(x => x.IsActive && x.Slug.ToLower() == normalizedSlug, cancellationToken);

        if (hasShowcase)
        {
            return new StorefrontSlugMatch(StorefrontSlugTargetType.Showcase, normalizedSlug);
        }

        var hasCategory = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(x => x.IsActive && x.Slug.ToLower() == normalizedSlug, cancellationToken);

        return hasCategory
            ? new StorefrontSlugMatch(StorefrontSlugTargetType.Category, normalizedSlug)
            : new StorefrontSlugMatch(StorefrontSlugTargetType.None, normalizedSlug);
    }

    private static SlugConflictException BuildConflict(string slug, SlugEntityType entityType, int entityId, string displayName)
    {
        var entityLabel = entityType switch
        {
            SlugEntityType.Category => "kategori",
            SlugEntityType.Product => "urun",
            SlugEntityType.Tag => "etiket",
            SlugEntityType.Showcase => "vitrin",
            _ => "kayit"
        };

        return new SlugConflictException(
            slug,
            $"'{slug}' slug'i zaten '{displayName}' {entityLabel} kaydinda kullaniliyor.",
            entityType,
            entityId);
    }

    private static string NormalizeSlug(string? slug)
        => (slug ?? string.Empty).Trim().ToLowerInvariant();

    private static async Task EnsureEntityConflictFreeAsync<TEntity>(
        IQueryable<TEntity> query,
        string normalizedSlug,
        SlugEntityType entityType,
        int? currentEntityId,
        Func<TEntity, int> idSelector,
        Func<TEntity, string> nameSelector,
        Func<TEntity, string> slugSelector,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var items = await query.ToListAsync(cancellationToken);
        var conflict = items.FirstOrDefault(entity =>
            (!currentEntityId.HasValue || idSelector(entity) != currentEntityId.Value) &&
            string.Equals(NormalizeSlug(slugSelector(entity)), normalizedSlug, StringComparison.Ordinal));

        if (conflict is not null)
        {
            throw BuildConflict(normalizedSlug, entityType, idSelector(conflict), nameSelector(conflict));
        }
    }
}
