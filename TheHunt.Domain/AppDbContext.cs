using Microsoft.EntityFrameworkCore;
using TheHunt.Domain.Models;

namespace TheHunt.Domain;

public class AppDbContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Members)
            .WithMany(m => m.Competitions)
            .UsingEntity(j => j.ToTable("competition_members"));

        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Verifiers)
            .WithMany(v => v.Competitions)
            .UsingEntity(j => j.ToTable("competition_verifiers"));

        base.OnModelCreating(modelBuilder);
    }
}