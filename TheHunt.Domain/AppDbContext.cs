using Microsoft.EntityFrameworkCore;
using TheHunt.Domain.Models;

namespace TheHunt.Domain;

public class AppDbContext : DbContext
{
    // public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}