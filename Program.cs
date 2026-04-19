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
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IHomeContentConfigurationService, HomeContentConfigurationService>();
builder.Services.AddScoped<IStorefrontContentService, StorefrontContentService>();
builder.Services.AddScoped<LocalAssetStorageService>();
builder.Services.AddScoped<S3CompatibleAssetStorageService>();
builder.Services.AddScoped<IAssetStorageService, AssetStorageService>();
builder.Services.AddSingleton<IMockContentService, MockContentService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<AppDbSeeder>();
    await seeder.SeedAsync();

    var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await identitySeeder.SeedAsync();
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
