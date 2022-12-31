using Discord;
using Discord.WebSocket;

namespace TheHunt.Bot.Extensions;

public static class DiscordSocketClientExtensions
{
    public static async Task StartAsync(this DiscordSocketClient client, string token)
    {
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }
}