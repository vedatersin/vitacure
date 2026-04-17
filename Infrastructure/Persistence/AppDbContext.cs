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
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<CustomerFavorite> CustomerFavorites => Set<CustomerFavorite>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerCartItem> CustomerCartItems => Set<CustomerCartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<HomeContentSettings> HomeContentSettings => Set<HomeContentSettings>();
    public DbSet<Showcase> Showcases => Set<Showcase>();
    public DbSet<ShowcaseCategory> ShowcaseCategories => Set<ShowcaseCategory>();
    public DbSet<ShowcaseFeaturedProduct> ShowcaseFeaturedProducts => Set<ShowcaseFeaturedProduct>();

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
            entity.Property(x => x.SeoTitle).HasMaxLength(300);
            entity.Property(x => x.MetaDescription).HasMaxLength(500);
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.OldPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Rating).HasColumnType("decimal(3,2)");
            entity.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.GalleryImageUrls).HasColumnType("nvarchar(max)");
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasIndex(x => new { x.AppUserId, x.ProductId }).IsUnique();

            entity.HasOne(x => x.AppUser)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.Property(x => x.TagsContent).HasColumnType("nvarchar(max)");
            entity.Property(x => x.BackgroundImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.IsDark).HasDefaultValue(true);
            entity.Property(x => x.SeoTitle).HasMaxLength(300);
            entity.Property(x => x.MetaDescription).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
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

    }
}
