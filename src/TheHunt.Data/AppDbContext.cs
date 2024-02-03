using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Data.Models;

namespace TheHunt.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Competition> Competitions => Set<Competition>();

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        Directory.CreateDirectory("data");

        using var scope = serviceProvider.CreateScope();
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}