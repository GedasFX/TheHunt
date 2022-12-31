using Microsoft.EntityFrameworkCore;
using TheHunt.Data.Models;

namespace TheHunt.Data;

public class AppDbContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}