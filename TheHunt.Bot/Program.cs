using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TheHunt.Bot;
using TheHunt.Bot.Extensions;
using TheHunt.Bot.Utils;
using TheHunt.Core;
using TheHunt.Data;
using TheHunt.Sheets;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace));

serviceCollection.AddDbContext<AppDbContext>(o => o
    .UseNpgsql("Server=127.0.0.1;Port=5432;Database=TheHunt;User Id=postgres;Password=example;Include Error Detail=true")
    .EnableSensitiveDataLogging());

serviceCollection.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect("localhost"));

serviceCollection.AddModule<BotModule>(configuration)
    .AddModule<SheetsModule>(configuration)
    .AddModule<DataModule>(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var discord = serviceProvider.GetRequiredService<DiscordSocketClient>();
await discord.StartAsync(configuration["Discord:Token"]!);

DiscordEventHandler.Register(discord, serviceProvider);
await DiscordEventHandler.RegisterInteractionsAsync(discord, serviceProvider);

await Task.Delay(-1);