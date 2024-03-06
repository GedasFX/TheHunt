using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheHunt.Core.Exceptions;

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
            await interactionService.RegisterCommandsToGuildAsync(ulong.Parse(
                serviceProvider.GetRequiredService<IConfiguration>()["DISCORD_DEBUG_GUILD_ID"] ??
                throw new InvalidOperationException("Missing DISCORD_DEBUG_GUILD_ID.")));
#else
            await interactionService.RegisterCommandsGloballyAsync();
#endif
        };

        var logger = serviceProvider.GetRequiredService<ILogger<InteractionService>>();
        interactionService.Log += message =>
        {
            if (message.Exception is InteractionException
                {
                    InnerException: EntityValidationException or EntityNotFoundException
                })
                return Task.CompletedTask;

            logger.Log((LogLevel)(5 - message.Severity), 0, message,
                message.Exception, delegate { return message.ToString(); });

            return Task.CompletedTask;
        };

        interactionService.ModalCommandExecuted += (_, context, result) => HandleInteractionException(context, result);
        interactionService.SlashCommandExecuted += (_, context, result) => HandleInteractionException(context, result);

        discord.InteractionCreated += c =>
            interactionService.ExecuteCommandAsync(new SocketInteractionContext(discord, c), serviceProvider);
    }

    private static async Task HandleInteractionException(IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess && result is ExecuteResult executeResult)
        {
            var exception = executeResult.Exception is InteractionException ie
                ? ie.InnerException
                : executeResult.Exception;

            var message = exception?.Message ?? result.ErrorReason;

            await (context.Interaction.HasResponded
                ? context.Interaction.FollowupAsync(message, ephemeral: true)
                : context.Interaction.RespondAsync(message, ephemeral: true));
        }
    }
}