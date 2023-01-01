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
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(l => l
#if DEBUG
            .AddConsole()
            .SetMinimumLevel(LogLevel.Trace)
#else
            .AddSimpleConsole()
            .SetMinimumLevel(LogLevel.Information)
            .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
#endif
    );

serviceCollection.AddDbContext<AppDbContext>(o => o
        .UseSqlite("Data Source=data/appdata.db")
#if DEBUG
        .EnableSensitiveDataLogging()
#endif
);

serviceCollection.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!));

serviceCollection.AddModule<BotModule>(configuration)
    .AddModule<SheetsModule>(configuration)
    .AddModule<DataModule>(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var discord = serviceProvider.GetRequiredService<DiscordSocketClient>();
await discord.StartAsync(configuration["Discord:Token"]!);

DiscordEventHandler.Register(discord, serviceProvider);
await DiscordEventHandler.RegisterInteractionsAsync(discord, serviceProvider);

_ = AppDbContext.Initialize(serviceProvider);

await Task.Delay(-1);