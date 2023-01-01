using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Data.Models;

namespace TheHunt.Data;

public class AppDbContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}