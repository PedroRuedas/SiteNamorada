using Microsoft.EntityFrameworkCore;
using SiteNamorada.Models;

namespace SiteNamorada.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Conteudo> Conteudos => Set<Conteudo>();
}