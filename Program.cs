using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.RateLimiting;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Identity;
using vitacure.Infrastructure.Persistence;
using vitacure.Infrastructure.Services;
using vitacure.Services.Content;

var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("StorefrontHome", policyBuilder => policyBuilder
        .Cache()
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("storefront"));

    options.AddPolicy("StorefrontCategory", policyBuilder => policyBuilder
        .Cache()
        .SetVaryByRouteValue("slug")
        .SetVaryByQuery("tag")
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("storefront")
        .Tag("category"));

    options.AddPolicy("StorefrontProduct", policyBuilder => policyBuilder
        .Cache()
        .SetVaryByRouteValue("slug")
        .Expire(TimeSpan.FromMinutes(10))
        .Tag("storefront")
        .Tag("product"));
});

var outputCacheStoreDescriptor = builder.Services.Single(service => service.ServiceType == typeof(IOutputCacheStore));

if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "Vitacure:";
    });
}

builder.Services.AddSingleton<ICacheObservabilityService, CacheObservabilityService>();
builder.Services.Remove(outputCacheStoreDescriptor);
builder.Services.AddSingleton<IOutputCacheStore>(serviceProvider =>
{
    var innerStore =
        outputCacheStoreDescriptor.ImplementationInstance as IOutputCacheStore ??
        outputCacheStoreDescriptor.ImplementationFactory?.Invoke(serviceProvider) as IOutputCacheStore ??
        (IOutputCacheStore)ActivatorUtilities.CreateInstance(serviceProvider, outputCacheStoreDescriptor.ImplementationType!);

    return new ObservedOutputCacheStore(
        innerStore,
        serviceProvider.GetRequiredService<ICacheObservabilityService>());
});

builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
    options.Cookie.Name = "Vitacure.Auth";
});
builder.Services.AddScoped<AppDbSeeder>();
builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<IAccountAccessService, AccountAccessService>();
builder.Services.AddScoped<ICustomerAccountService, CustomerAccountService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IGuestSessionService, GuestSessionService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
builder.Services.AddScoped<IRedisConnectionStatusService, RedisConnectionStatusService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
builder.Services.AddScoped<IAdminBrandService, AdminBrandService>();
builder.Services.AddScoped<IAdminCollectionService, AdminCollectionService>();
builder.Services.AddScoped<IAdminFeatureService, AdminFeatureService>();
builder.Services.AddScoped<IAdminMediaLibraryService, AdminMediaLibraryService>();
builder.Services.AddScoped<IAdminStorageSettingsService, AdminStorageSettingsService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IAdminTagService, AdminTagService>();
builder.Services.AddScoped<IAdminShowcaseService, AdminShowcaseService>();
builder.Services.AddScoped<IAdminHomeContentService, AdminHomeContentService>();
builder.Services.AddScoped<IkasCatalogImportService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IHomeContentConfigurationService, HomeContentConfigurationService>();
builder.Services.AddScoped<IStorefrontContentService, StorefrontContentService>();
builder.Services.AddScoped<LocalAssetStorageService>();
builder.Services.AddScoped<S3CompatibleAssetStorageService>();
builder.Services.AddScoped<IAssetStorageService, AssetStorageService>();

var app = builder.Build();
var ikasImportJsonPath = ResolveImportJsonPath(args);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await EnsureProductSchemaHotfixAsync(dbContext);
    await EnsureGoogleProductCategorySchemaHotfixAsync(dbContext);
    await EnsureProductExperienceSchemaHotfixAsync(dbContext);

    if (!string.IsNullOrWhiteSpace(ikasImportJsonPath))
    {
        var importer = scope.ServiceProvider.GetRequiredService<IkasCatalogImportService>();
        await importer.ImportFromJsonAsync(ikasImportJsonPath);
        return;
    }
    else
    {
        var seeder = scope.ServiceProvider.GetRequiredService<AppDbSeeder>();
        await seeder.SeedAsync();

        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await identitySeeder.SeedAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllerRoute(
    name: "admin_area",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "legacy_product",
    pattern: "urun/{slug}",
    defaults: new { controller = "Product", action = "Detail" });

app.MapControllerRoute(
    name: "slug",
    pattern: "{slug}",
    defaults: new { controller = "Slug", action = "Resolve" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string? ResolveImportJsonPath(IEnumerable<string> args)
{
    var values = args.ToArray();
    for (var index = 0; index < values.Length; index++)
    {
        var current = values[index];
        if (string.Equals(current, "--import-ikas-json", StringComparison.OrdinalIgnoreCase))
        {
            return index + 1 < values.Length ? values[index + 1] : null;
        }

        const string prefix = "--import-ikas-json=";
        if (current.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return current[prefix.Length..];
        }
    }

    return null;
}

static async Task EnsureProductSchemaHotfixAsync(AppDbContext dbContext)
{
    const string migrationId = "20260426121500_AddProductKindAndUnitPricingFields";
    const string productVersion = "8.0.11";

    await dbContext.Database.ExecuteSqlRawAsync(
        $$"""
        IF COL_LENGTH('dbo.Products', 'ProductKind') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [ProductKind] nvarchar(24) NOT NULL CONSTRAINT [DF_Products_ProductKind_Hotfix] DEFAULT N'Physical';
        END

        IF COL_LENGTH('dbo.Products', 'ReviewCount') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [ReviewCount] int NOT NULL CONSTRAINT [DF_Products_ReviewCount_Hotfix] DEFAULT 0;
        END

        IF COL_LENGTH('dbo.Products', 'UnitComparisonAmount') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [UnitComparisonAmount] decimal(12,4) NULL;
        END

        IF COL_LENGTH('dbo.Products', 'UnitComparisonType') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [UnitComparisonType] nvarchar(16) NULL;
        END

        IF COL_LENGTH('dbo.Products', 'UnitContentAmount') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [UnitContentAmount] decimal(12,4) NULL;
        END

        IF COL_LENGTH('dbo.Products', 'UnitContentType') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [UnitContentType] nvarchar(16) NULL;
        END

        IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'{{migrationId}}')
        BEGIN
            INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
            VALUES (N'{{migrationId}}', N'{{productVersion}}');
        END
        """);
}

static async Task EnsureGoogleProductCategorySchemaHotfixAsync(AppDbContext dbContext)
{
    await dbContext.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'[dbo].[GoogleProductCategories]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[GoogleProductCategories]
            (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Name] NVARCHAR(200) NOT NULL,
                [Slug] NVARCHAR(200) NOT NULL,
                [ParentId] INT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_GoogleProductCategories_IsActive_Hotfix] DEFAULT 1,
                [SortOrder] INT NOT NULL CONSTRAINT [DF_GoogleProductCategories_SortOrder_Hotfix] DEFAULT 0
            );

            CREATE UNIQUE INDEX [IX_GoogleProductCategories_Slug] ON [dbo].[GoogleProductCategories]([Slug]);
        END

        IF COL_LENGTH('dbo.Products', 'GoogleProductCategoryId') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Products] ADD [GoogleProductCategoryId] int NULL;
        END
        """);
}

static async Task EnsureProductExperienceSchemaHotfixAsync(AppDbContext dbContext)
{
    await dbContext.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'[dbo].[CustomFieldDefinitions]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[CustomFieldDefinitions]
            (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Name] NVARCHAR(150) NOT NULL,
                [Slug] NVARCHAR(150) NOT NULL,
                [FieldType] NVARCHAR(64) NOT NULL,
                [IsFilterable] BIT NOT NULL CONSTRAINT [DF_CustomFieldDefinitions_IsFilterable_Hotfix] DEFAULT 0,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_CustomFieldDefinitions_IsActive_Hotfix] DEFAULT 1
            );

            CREATE UNIQUE INDEX [IX_CustomFieldDefinitions_Slug] ON [dbo].[CustomFieldDefinitions]([Slug]);
        END

        IF OBJECT_ID(N'[dbo].[PersonalizationDefinitions]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[PersonalizationDefinitions]
            (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Name] NVARCHAR(150) NOT NULL,
                [Slug] NVARCHAR(150) NOT NULL,
                [InputType] NVARCHAR(64) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_PersonalizationDefinitions_IsActive_Hotfix] DEFAULT 1
            );

            CREATE UNIQUE INDEX [IX_PersonalizationDefinitions_Slug] ON [dbo].[PersonalizationDefinitions]([Slug]);
        END

        IF OBJECT_ID(N'[dbo].[ProductCustomFields]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ProductCustomFields]
            (
                [ProductId] INT NOT NULL,
                [CustomFieldDefinitionId] INT NOT NULL,
                CONSTRAINT [PK_ProductCustomFields] PRIMARY KEY ([ProductId], [CustomFieldDefinitionId])
            );
        END

        IF OBJECT_ID(N'[dbo].[ProductPersonalizations]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ProductPersonalizations]
            (
                [ProductId] INT NOT NULL,
                [PersonalizationDefinitionId] INT NOT NULL,
                CONSTRAINT [PK_ProductPersonalizations] PRIMARY KEY ([ProductId], [PersonalizationDefinitionId])
            );
        END
        """);
}
