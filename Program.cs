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

EnsureDatabaseExists(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
var app = builder.Build();
// Ensure database tables are created on startup
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

static void EnsureDatabaseExists(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var databaseName = builder.Database;

    if (string.IsNullOrWhiteSpace(databaseName))
    {
        throw new InvalidOperationException("The PostgreSQL connection string must include a database name.");
    }

    var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Database = "postgres"
    };

    using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
    connection.Open();

    using var existsCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
    existsCommand.Parameters.AddWithValue("name", databaseName);
    var exists = existsCommand.ExecuteScalar() != null;

    if (!exists)
    {
        using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        createCommand.ExecuteNonQuery();
    }
}

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