using Microsoft.EntityFrameworkCore;
using Npgsql;
using SiteNamorada.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    connectionString = ConvertDatabaseUrl(databaseUrl);
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("PostgreSQL connection string is not configured. Set ConnectionStrings:DefaultConnection in appsettings.json or the DATABASE_URL environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
var app = builder.Build();
// Ensure database and tables are created on startup
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

app.Run();

static string ConvertDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Prefer
    }.ConnectionString;
}