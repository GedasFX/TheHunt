using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TheHunt.Bot;

public static class TheHuntDependencyInjection
{
    public static IServiceCollection AddDiscord(this IServiceCollection collection, string botToken)
    {
        return collection.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClientWrapper(sp, botToken));
    }


    private class DiscordSocketClientWrapper : DiscordSocketClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _botToken;

        public DiscordSocketClientWrapper(IServiceProvider serviceProvider, string botToken) : base(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.None
        })
        {
            _serviceProvider = serviceProvider;
            _botToken = botToken;

            var logger = serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();
            Log += message =>
            {
                logger.Log((LogLevel)Math.Abs((int)message.Severity - 5), 0, message,
                    message.Exception, delegate { return message.ToString(); });
                return Task.CompletedTask;
            };
        }

        public override async Task StartAsync()
        {
            await LoginAsync(TokenType.Bot, _botToken);
            await base.StartAsync();

            SlashCommandHandler.Register(this, _serviceProvider);
        }
    }
}