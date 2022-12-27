using Microsoft.EntityFrameworkCore;
using TheHunt.Domain.Models;

namespace TheHunt.Domain;

public class AppDbContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<CompetitionUser> CompetitionUsers => Set<CompetitionUser>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompetitionUser>()
            .HasKey(e => new { e.CompetitionId, e.UserId });

        base.OnModelCreating(modelBuilder);
    }
}