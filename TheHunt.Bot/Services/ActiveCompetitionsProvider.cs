using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Domain;

namespace TheHunt.Bot.Services;

public class ActiveCompetitionsProvider
{
    private readonly IServiceProvider _serviceProvider;

    public ConcurrentDictionary<ulong, DateTime?> ActiveChannelMap { get; private set; } = new();

    public ActiveCompetitionsProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        Task.Run(async () =>
        {
            while (true)
            {
                await Initialize();
                await Task.Delay(TimeSpan.FromDays(1));
            }

            // ReSharper disable once FunctionNeverReturns
        });
    }

    private async Task Initialize()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await foreach (var dbSubmission in context.Competitions
                           .Where(c => c.EndDate == null || c.EndDate > DateTime.UtcNow)
                           .Select(c => new { c.SubmissionChannelId, c.EndDate }).AsAsyncEnumerable())
        {
            ActiveChannelMap.AddOrUpdate(dbSubmission.SubmissionChannelId, dbSubmission.EndDate, (_, v) => v);
        }
    }
}