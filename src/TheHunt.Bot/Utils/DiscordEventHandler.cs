using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheHunt.Core.Exceptions;
using TheHunt.Data;
using TheHunt.Data.Models;

namespace TheHunt.Bot.Utils;

public static class DiscordEventHandler
{
    public static void Register(DiscordSocketClient client, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();
        client.Log += message =>
        {
            logger.Log((LogLevel)(5 - message.Severity), 0, message,
                message.Exception, delegate { return message.ToString(); });
            return Task.CompletedTask;
        };

        client.ChannelDestroyed += async channel =>
        {
            if (channel is not ITextChannel)
                return;

            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await dbContext.Competitions.AnyAsync(c => c.ChannelId == channel.Id))
            {
                dbContext.Competitions.Remove(new Competition() { ChannelId = channel.Id });
                await dbContext.SaveChangesAsync();
            }
        };
    }

    public static async Task RegisterInteractionsAsync(DiscordSocketClient discord, IServiceProvider serviceProvider)
    {
        var interactionService = new InteractionService(discord, new InteractionServiceConfig()
        {
            UseCompiledLambda = true, DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Verbose,
            AutoServiceScopes = true, InteractionCustomIdDelimiters = [' ']
        });
        await interactionService.AddModulesAsync(typeof(Program).Assembly, serviceProvider);

        discord.Ready += async () =>
        {
#if DEBUG
            await interactionService.RegisterCommandsToGuildAsync(609728856211062785);
#else
            await interactionService.RegisterCommandsGloballyAsync();
#endif
        };

        var logger = serviceProvider.GetRequiredService<ILogger<InteractionService>>();
        interactionService.Log += message =>
        {
            if (message.Exception is EntityValidationException)
                return Task.CompletedTask;

            logger.Log((LogLevel)(5 - message.Severity), 0, message,
                message.Exception, delegate { return message.ToString(); });

            return Task.CompletedTask;
        };

        interactionService.ModalCommandExecuted += (_, context, result) => HandleInteractionException(context, result);
        interactionService.SlashCommandExecuted += (_, context, result) => HandleInteractionException(context, result);

        discord.InteractionCreated += c => interactionService.ExecuteCommandAsync(new SocketInteractionContext(discord, c), serviceProvider);
    }

    private static async Task HandleInteractionException(IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            await (context.Interaction.HasResponded
                ? context.Interaction.FollowupAsync(result.ErrorReason, ephemeral: true)
                : context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true));
        }
    }
}