using Discord;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheHunt.Application;
using TheHunt.Bot;
using TheHunt.Bot.Internal;
using TheHunt.Domain;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace));

serviceCollection.AddApplication(configuration)
    .AddDiscord(configuration["Discord:Token"]!);
serviceCollection.AddSingleton(_ => new MySheet("google.json"));

serviceCollection.AddHttpClient<CdnHttpClient>();

var serviceProvider = serviceCollection.BuildServiceProvider();
// await new MySheet().Playground();
var discord = serviceProvider.GetRequiredService<DiscordSocketClient>();
await discord.StartAsync();
 
var channelCache = new Dictionary<ulong, bool>();
using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await foreach (var competition in context.Competitions
                       .Where(c => c.EndDate == null || c.EndDate > DateTime.UtcNow)
                       .Select(c => new { c.ChannelId }).AsAsyncEnumerable())
    {
        channelCache.Add(competition.ChannelId, true);
    }
}


discord.MessageReceived += async message =>
{
    if (message.Attachments.Count == 0 && message.Embeds.Count == 0)
        return;

    if (!channelCache.ContainsKey(message.Channel.Id))
        return;

    // Message is intended as a competition submission.

    var embed = new EmbedBuilder()
        .WithAuthor(message.Author)
        .WithCurrentTimestamp()
        .Build();

    foreach (var attachment in message.Attachments)
    {
        if (!attachment.ContentType.StartsWith("image/"))
            continue;

        // await using var stream = await serviceProvider.GetRequiredService<CdnHttpClient>().GetImageStream(attachment.Url);
        // await message.Channel.SendFileAsync(stream, attachment.Filename, embed: embed);
    }

    foreach (var embed1 in message.Embeds)
    {
    }
};

DiscordEventHandler.Register(discord, serviceProvider);
await DiscordEventHandler.RegisterInteractionsAsync(discord, serviceProvider);

await Task.Delay(-1);