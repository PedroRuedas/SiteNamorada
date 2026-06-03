using Microsoft.EntityFrameworkCore;
using SiteNamorada.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dbPath = Path.Combine(AppContext.BaseDirectory, "site.db");
    options.UseSqlite($"Data Source={dbPath}");
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
var app = builder.Build();
// Ensure database and tables are created on startup (creates site.db and tables if missing)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Use default ASP.NET Core URL configuration (launchSettings, ASPNETCORE_URLS or expando use)
app.Run();