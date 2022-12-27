using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheHunt.Application;
using TheHunt.Bot;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var serviceProvider = new ServiceCollection()
    .AddLogging(l => l.AddConsole())
    .AddApplication(configuration)
    .AddDiscord(configuration["Discord:Token"])
    .BuildServiceProvider();

var discord = serviceProvider.GetRequiredService<DiscordSocketClient>();
await discord.StartAsync();
discord.Ready += async () =>
{
    var guild = discord.GetGuild(609728856211062785);

    await guild.CreateApplicationCommandAsync(new SlashCommandBuilder()
        .WithName("competitions")
        .WithDescription("Tools to manage competitions.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("help").WithDescription("Displays Help for this command."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("create").WithDescription("Creates a new competition and binds it to the channel.")
            .AddOption("name", ApplicationCommandOptionType.String, "Name of the competition.")
            .AddOption("description", ApplicationCommandOptionType.String, "Short Description about the competition.")
            .AddOption("start_date", ApplicationCommandOptionType.String, "Start date for the competition. Example: '2022-12-26 21:30:00' (without quotes).")
            .AddOption("end_date", ApplicationCommandOptionType.String, "End date for the competition. Example: '2022-12-26 21:30:00' (without quotes)."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("update").WithDescription("Updates the competition bound to the selected channel."))
        .Build());

    // await guild.CreateApplicationCommandAsync(new SlashCommandBuilder().WithName("help").WithDescription("Shows help.").Build());
};
await Task.Delay(-1);