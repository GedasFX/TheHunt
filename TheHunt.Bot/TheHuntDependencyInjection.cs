using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TheHunt.Bot;

public static class TheHuntDependencyInjection
{
    public static void AddDiscord(this IServiceCollection collection, string botToken)
    {
        collection.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClientWrapper(sp, botToken));
    }


    private class DiscordSocketClientWrapper : DiscordSocketClient
    {
        public DiscordSocketClientWrapper(IServiceProvider serviceProvider, string botToken) : base(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.None
        })
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();
            Log += message =>
            {
                logger.Log((LogLevel)Math.Abs((int)message.Severity - 5), 0, message,
                    message.Exception, delegate { return message.ToString(); });
                return Task.CompletedTask;
            };

            SlashCommandHandler.Register(this, serviceProvider);

            Run(botToken).GetAwaiter().GetResult();
        }

        private async Task Run(string token)
        {
            await LoginAsync(TokenType.Bot, token);
            await StartAsync();
        }
    }
}