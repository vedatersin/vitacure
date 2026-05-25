using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;

namespace vitacure.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<GoogleProductCategory> GoogleProductCategories => Set<GoogleProductCategory>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductBundleItem> ProductBundleItems => Set<ProductBundleItem>();
    public DbSet<ProductSavedFilter> ProductSavedFilters => Set<ProductSavedFilter>();
    public DbSet<StorageSettings> StorageSettings => Set<StorageSettings>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductMedia> ProductMedias => Set<ProductMedia>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductVariantGroup> ProductVariantGroups => Set<ProductVariantGroup>();
    public DbSet<ProductVariantOption> ProductVariantOptions => Set<ProductVariantOption>();
    public DbSet<ProductVariantSelection> ProductVariantSelections => Set<ProductVariantSelection>();
    public DbSet<ProductCollection> ProductCollections => Set<ProductCollection>();
    public DbSet<ProductCustomField> ProductCustomFields => Set<ProductCustomField>();
    public DbSet<ProductFeature> ProductFeatures => Set<ProductFeature>();
    public DbSet<PersonalizationDefinition> PersonalizationDefinitions => Set<PersonalizationDefinition>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductPersonalization> ProductPersonalizations => Set<ProductPersonalization>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<CustomerFavorite> CustomerFavorites => Set<CustomerFavorite>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerCartItem> CustomerCartItems => Set<CustomerCartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();
    public DbSet<HomeContentSettings> HomeContentSettings => Set<HomeContentSettings>();
    public DbSet<Showcase> Showcases => Set<Showcase>();
    public DbSet<ShowcaseCategory> ShowcaseCategories => Set<ShowcaseCategory>();
    public DbSet<ShowcaseFeaturedProduct> ShowcaseFeaturedProducts => Set<ShowcaseFeaturedProduct>();
    public DbSet<ShowcasePrompt> ShowcasePrompts => Set<ShowcasePrompt>();
    public DbSet<ShowcaseTag> ShowcaseTags => Set<ShowcaseTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.AccountType)
                .HasConversion(
                    value => value.ToString(),
                    value => Enum.Parse<AccountType>(value))
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(x => x.IsBackOfficeRole).IsRequired();
        });

        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.ProductSortType).HasMaxLength(64);
            entity.Property(x => x.SeoTitle).HasMaxLength(300);
            entity.Property(x => x.MetaDescription).HasMaxLength(500);
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("Collections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ShowOnHome).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.ToTable("Features");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.GroupName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.OptionsContent).HasColumnType("nvarchar(max)");
            entity.Property(x => x.IsActive).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<CustomFieldDefinition>(entity =>
        {
            entity.ToTable("CustomFieldDefinitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.FieldType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IsFilterable).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<PersonalizationDefinition>(entity =>
        {
            entity.ToTable("PersonalizationDefinitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.InputType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<GoogleProductCategory>(entity =>
        {
            entity.ToTable("GoogleProductCategories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.ToTable("MediaAssets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.AltText).HasMaxLength(250);
            entity.Property(x => x.StorageProvider).HasMaxLength(50).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Url).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
            entity.Property(x => x.SizeBytes).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.Url).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<StorageSettings>(entity =>
        {
            entity.ToTable("StorageSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Provider)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(x => x.IsCdnEnabled).IsRequired();
            entity.Property(x => x.ServiceUrl).HasMaxLength(500);
            entity.Property(x => x.PublicBaseUrl).HasMaxLength(500);
            entity.Property(x => x.BucketName).HasMaxLength(200);
            entity.Property(x => x.Region).HasMaxLength(100);
            entity.Property(x => x.AccessKey).HasMaxLength(200);
            entity.Property(x => x.SecretKey).HasMaxLength(300);
            entity.Property(x => x.KeyPrefix).HasMaxLength(200);
            entity.Property(x => x.UsePathStyle).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKind)
                .HasConversion<string>()
                .HasMaxLength(24)
                .HasDefaultValue(ProductKind.Physical)
                .IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.Property(x => x.MetaTitle).HasMaxLength(300);
            entity.Property(x => x.MetaDescription).HasMaxLength(500);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.OldPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Rating).HasColumnType("decimal(3,2)");
            entity.Property(x => x.ReviewCount).HasDefaultValue(0);
            entity.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.GalleryImageUrls).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Sku).HasMaxLength(100);
            entity.Property(x => x.Barcode).HasMaxLength(100);
            entity.Property(x => x.Desi).HasColumnType("decimal(10,2)");
            entity.Property(x => x.HsCode).HasMaxLength(64);
            entity.Property(x => x.SupplierName).HasMaxLength(160);
            entity.Property(x => x.ContinueSellingWhenOutOfStock).IsRequired();
            entity.Property(x => x.BundleMode).HasMaxLength(24);
            entity.Property(x => x.BundlePricingMode).HasMaxLength(24);
            entity.Property(x => x.BundleAdjustmentType).HasMaxLength(24);
            entity.Property(x => x.BundleAdjustmentAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.ShowUnitPrice).IsRequired();
            entity.Property(x => x.UnitContentAmount).HasColumnType("decimal(12,4)");
            entity.Property(x => x.UnitContentType).HasMaxLength(16);
            entity.Property(x => x.UnitComparisonAmount).HasColumnType("decimal(12,4)");
            entity.Property(x => x.UnitComparisonType).HasMaxLength(16);
            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(ProductPublishingStatus.PublishedOpen)
                .IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Brand)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.GoogleProductCategory)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.GoogleProductCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductSavedFilter>(entity =>
        {
            entity.ToTable("ProductSavedFilters");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.FiltersJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.SortOrder });

            entity.HasOne(x => x.User)
                .WithMany(x => x.ProductSavedFilters)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductMedia>(entity =>
        {
            entity.ToTable("ProductMedias");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Url).HasMaxLength(500).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(250);
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.IsPrimary).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.ProductId, x.SortOrder });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductMedias)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MediaAsset)
                .WithMany()
                .HasForeignKey(x => x.MediaAssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("ProductVariants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(240).IsRequired();
            entity.Property(x => x.GroupName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OptionName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.Sku).HasMaxLength(100);
            entity.Property(x => x.Barcode).HasMaxLength(120);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.OldPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Stock).IsRequired();
            entity.Property(x => x.Desi).HasColumnType("decimal(18,2)");
            entity.Property(x => x.HsCode).HasMaxLength(50);
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.IsDefault).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductBundleItem>(entity =>
        {
            entity.ToTable("ProductBundleItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntryMode).HasMaxLength(24).IsRequired();
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.ProductId, x.ProductVariantId, x.SortOrder });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductBundleItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.ChildProduct)
                .WithMany()
                .HasForeignKey(x => x.ChildProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChildProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ChildProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariantGroup>(entity =>
        {
            entity.ToTable("ProductVariantGroups");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SelectionStyle).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ShowOnCard).IsRequired();
            entity.Property(x => x.IsPrimary).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductVariantGroups)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariantOption>(entity =>
        {
            entity.ToTable("ProductVariantOptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ColorHex).HasMaxLength(20);
            entity.Property(x => x.SwatchImageUrl).HasMaxLength(500);
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.ProductVariantGroup)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.ProductVariantGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariantSelection>(entity =>
        {
            entity.ToTable("ProductVariantSelections");
            entity.HasKey(x => new { x.ProductVariantId, x.ProductVariantOptionId });

            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.Selections)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProductVariantOption)
                .WithMany(x => x.VariantSelections)
                .HasForeignKey(x => x.ProductVariantOptionId)
                .OnDelete(DeleteBehavior.ClientCascade);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(x => new { x.ProductId, x.CategoryId });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductFeature>(entity =>
        {
            entity.ToTable("ProductFeatures");
            entity.HasKey(x => new { x.ProductId, x.FeatureId });
            entity.Property(x => x.Value).HasMaxLength(150);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductFeatures)
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.Feature)
                .WithMany(x => x.ProductFeatures)
                .HasForeignKey(x => x.FeatureId);
        });

        modelBuilder.Entity<ProductCustomField>(entity =>
        {
            entity.ToTable("ProductCustomFields");
            entity.HasKey(x => new { x.ProductId, x.CustomFieldDefinitionId });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductCustomFields)
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.CustomFieldDefinition)
                .WithMany(x => x.ProductCustomFields)
                .HasForeignKey(x => x.CustomFieldDefinitionId);
        });

        modelBuilder.Entity<ProductPersonalization>(entity =>
        {
            entity.ToTable("ProductPersonalizations");
            entity.HasKey(x => new { x.ProductId, x.PersonalizationDefinitionId });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductPersonalizations)
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.PersonalizationDefinition)
                .WithMany(x => x.ProductPersonalizations)
                .HasForeignKey(x => x.PersonalizationDefinitionId);
        });

        modelBuilder.Entity<ProductCollection>(entity =>
        {
            entity.ToTable("ProductCollections");
            entity.HasKey(x => new { x.ProductId, x.CollectionId });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductCollections)
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.Collection)
                .WithMany(x => x.ProductCollections)
                .HasForeignKey(x => x.CollectionId);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.ToTable("ProductTags");
            entity.HasKey(x => new { x.ProductId, x.TagId });

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductTags)
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.Tag)
                .WithMany(x => x.ProductTags)
                .HasForeignKey(x => x.TagId);
        });

        modelBuilder.Entity<CustomerFavorite>(entity =>
        {
            entity.ToTable("CustomerFavorites");
            entity.HasKey(x => new { x.AppUserId, x.ProductId });
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasOne(x => x.AppUser)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.ToTable("CustomerAddresses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RecipientName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
            entity.Property(x => x.District).HasMaxLength(100).IsRequired();
            entity.Property(x => x.AddressLine).HasColumnType("nvarchar(max)");
            entity.Property(x => x.PostalCode).HasMaxLength(20);
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasOne(x => x.AppUser)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerCartItem>(entity =>
        {
            entity.ToTable("CustomerCartItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.AppUserId, x.ProductId, x.ProductVariantId });

            entity.HasOne(x => x.AppUser)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.RecipientName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
            entity.Property(x => x.District).HasMaxLength(100).IsRequired();
            entity.Property(x => x.AddressLine).HasColumnType("nvarchar(max)");
            entity.Property(x => x.PostalCode).HasMaxLength(20);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.OrderNumber).IsUnique();

            entity.HasOne(x => x.AppUser)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProductSlug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.VariantLabel).HasMaxLength(160);
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdminNotification>(entity =>
        {
            entity.ToTable("AdminNotifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Body).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Actor).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CategoryKey).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetLabel).HasMaxLength(100);
            entity.Property(x => x.TargetUrl).HasMaxLength(500);
            entity.Property(x => x.OccurredAt).IsRequired();
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => new { x.CategoryKey, x.IsRead });
        });

        modelBuilder.Entity<HomeContentSettings>(entity =>
        {
            entity.ToTable("HomeContentSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MetaDescription).HasMaxLength(500);
            entity.Property(x => x.HeroTitle).HasMaxLength(200).IsRequired();
            entity.Property(x => x.HeroSubtitle).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MainPlaceholder).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.SearchPlaceholder).HasMaxLength(300).IsRequired();
            entity.Property(x => x.SearchPlaceholderLocked).HasMaxLength(300).IsRequired();
            entity.Property(x => x.FeaturedTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.FeaturedActionLabel).HasMaxLength(100);
            entity.Property(x => x.FeaturedActionUrl).HasMaxLength(500);
            entity.Property(x => x.PopularTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CampaignsTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DealsTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DealsActionLabel).HasMaxLength(100);
            entity.Property(x => x.DealsActionUrl).HasMaxLength(500);
            entity.Property(x => x.FeaturedBannerName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.FeaturedBannerAltText).HasMaxLength(150).IsRequired();
            entity.Property(x => x.FeaturedBannerImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FeaturedBannerTargetUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.PopularSupplementsContent).HasColumnType("nvarchar(max)");
            entity.Property(x => x.CampaignBannersContent).HasColumnType("nvarchar(max)");
            entity.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Showcase>(entity =>
        {
            entity.ToTable("Showcases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IconClass).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IconColor).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.Property(x => x.TagsContent).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ExamplePromptsContent).HasColumnType("nvarchar(max)");
            entity.Property(x => x.BackgroundImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.IsDark).HasDefaultValue(true);
            entity.Property(x => x.SeoTitle).HasMaxLength(300);
            entity.Property(x => x.MetaDescription).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.PrimaryCategory)
                .WithMany(x => x.PrimaryShowcases)
                .HasForeignKey(x => x.PrimaryCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShowcaseCategory>(entity =>
        {
            entity.ToTable("ShowcaseCategories");
            entity.HasKey(x => new { x.ShowcaseId, x.CategoryId });

            entity.HasOne(x => x.Showcase)
                .WithMany(x => x.ShowcaseCategories)
                .HasForeignKey(x => x.ShowcaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.ShowcaseCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShowcaseFeaturedProduct>(entity =>
        {
            entity.ToTable("ShowcaseFeaturedProducts");
            entity.HasKey(x => new { x.ShowcaseId, x.ProductId });
            entity.Property(x => x.SortOrder).IsRequired();

            entity.HasOne(x => x.Showcase)
                .WithMany(x => x.FeaturedProducts)
                .HasForeignKey(x => x.ShowcaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ShowcaseFeaturedProducts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShowcasePrompt>(entity =>
        {
            entity.ToTable("ShowcasePrompts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(500).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.HasIndex(x => new { x.ShowcaseId, x.SortOrder });

            entity.HasOne(x => x.Showcase)
                .WithMany(x => x.Prompts)
                .HasForeignKey(x => x.ShowcaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShowcaseTag>(entity =>
        {
            entity.ToTable("ShowcaseTags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.HasIndex(x => new { x.ShowcaseId, x.SortOrder });

            entity.HasOne(x => x.Showcase)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.ShowcaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
