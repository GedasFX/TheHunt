using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TheHunt.Application;
using TheHunt.Bot;
using TheHunt.Bot.Services;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace));

serviceCollection.AddApplication(configuration)
    .AddDiscord(configuration["Discord:Token"]!);
serviceCollection.AddSingleton(_ => new SpreadsheetService("google.json"));
serviceCollection.AddScoped<SpreadsheetQueryService>();
serviceCollection.AddScoped<CompetitionsQueryService>();
serviceCollection.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost"));

var serviceProvider = serviceCollection.BuildServiceProvider();

var discord = serviceProvider.GetRequiredService<DiscordSocketClient>();
await discord.StartAsync();

DiscordEventHandler.Register(discord, serviceProvider);
await DiscordEventHandler.RegisterInteractionsAsync(discord, serviceProvider);

await Task.Delay(-1);