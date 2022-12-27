using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheHunt.Application;
using TheHunt.Bot;

async Task HandleInteractionException(IInteractionContext context, IResult result)
{
    if (!result.IsSuccess)
    {
        await (context.Interaction.HasResponded
            ? context.Interaction.FollowupAsync(result.ErrorReason, ephemeral: true)
            : context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true));
    }
}

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var serviceProvider = new ServiceCollection()
    .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace))
    .AddApplication(configuration)
    .AddDiscord(configuration["Discord:Token"])
    .BuildServiceProvider();

var discord = serviceProvider.GetRequiredService<DiscordSocketClient>();
await discord.StartAsync();

var interactionService = new InteractionService(discord, new InteractionServiceConfig()
{
    UseCompiledLambda = true, DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Verbose,
    AutoServiceScopes = true, InteractionCustomIdDelimiters = new[] { ' ' }
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
    logger.Log((LogLevel)(5 - message.Severity), 0, message,
        message.Exception, delegate { return message.ToString(); });
    return Task.CompletedTask;
};

interactionService.ModalCommandExecuted += (_, context, result) => HandleInteractionException(context, result);
interactionService.SlashCommandExecuted += (_, context, result) => HandleInteractionException(context, result);
discord.InteractionCreated += c => interactionService.ExecuteCommandAsync(new SocketInteractionContext(discord, c), serviceProvider);

// discord.Ready += async () =>
// {
//     var guild = discord.GetGuild(609728856211062785);
//
//     await guild.CreateApplicationCommandAsync(new SlashCommandBuilder()
//         .WithName("competitions")
//         .WithDescription("Tools to manage competitions.")
//         .AddOption(new SlashCommandOptionBuilder()
//             .WithType(ApplicationCommandOptionType.SubCommand)
//             .WithName("help").WithDescription("Displays Help for this command."))
//         .AddOption(new SlashCommandOptionBuilder()
//             .WithType(ApplicationCommandOptionType.SubCommand)
//             .WithName("create").WithDescription("Creates a new competition and binds it to the channel.")
//             .AddOption("name", ApplicationCommandOptionType.String, "Name of the competition.")
//             .AddOption("description", ApplicationCommandOptionType.String, "Short Description about the competition.")
//             .AddOption("start_date", ApplicationCommandOptionType.String, "Start date for the competition. Example: '2022-12-26 21:30:00' (without quotes).")
//             .AddOption("end_date", ApplicationCommandOptionType.String, "End date for the competition. Example: '2022-12-26 21:30:00' (without quotes)."))
//         .AddOption(new SlashCommandOptionBuilder()
//             .WithType(ApplicationCommandOptionType.SubCommand)
//             .WithName("update").WithDescription("Updates the competition bound to the selected channel."))
//         .Build());
//
//     // await guild.CreateApplicationCommandAsync(new SlashCommandBuilder().WithName("help").WithDescription("Shows help.").Build());
// };
await Task.Delay(-1);