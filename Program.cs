using Microsoft.AspNetCore.StaticFiles;
using vitacure.Services.Content;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMockContentService, MockContentService>();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "category",
    pattern: "{slug:regex(^(uyku-sagligi|multivitamin-enerji|zihin-hafiza-guclendirme|hastaliklara-karsi-koruma|kas-ve-iskelet-sagligi|zayiflama-destegi)$)}",
    defaults: new { controller = "Category", action = "Detail" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
