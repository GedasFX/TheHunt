using System.Globalization;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Application.Features.Competition;
using TheHunt.Application.Helpers;

namespace TheHunt.Bot;

public class SlashCommandHandler
{
    public static void Register(DiscordSocketClient client, IServiceProvider serviceProvider)
    {
        var handler = new SlashCommandHandler();
        client.SlashCommandExecuted += async command =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<IRequestContextAccessor>().Context = new UserData()
            {
                Username = command.User.Username,
                UserId = (long)command.User.Id,
            };

            try
            {
                // await command.DeferAsync();
                await OnSlashCommandExecuted(scope.ServiceProvider.GetRequiredService<IMediator>(), command);
            }
            catch (Exception e)
            {
                await command.FollowupAsync(e.Message);
                throw;
            }
        };
    }

    private static Task OnSlashCommandExecuted(IMediator mediator, SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "competitions":
            {
                switch (command.Data.Options.FirstOrDefault()?.Name)
                {
                    case "help": return HandleCompetitionsHelp(command);
                    default:
                        throw new InvalidOperationException(
                            $"Command '{command.CommandName} {command.Data.Options.FirstOrDefault()?.Name}' does not have a handler.");
                }
            }
            case "create": return HandleCreate(mediator, command);
            default: throw new InvalidOperationException($"Command '{command.CommandName}' does not have a handler.");
        }
    }

    private static async Task HandleCompetitionsHelp(IDiscordInteraction command)
    {
        // _ = command.DeferAsync();
        await command.RespondWithModalAsync(new ModalBuilder().WithTitle("Yoo").WithCustomId("1515").Build());
    }

    private static async Task HandleCreate(ISender mediator, SocketSlashCommand command)
    {
        var resp = await mediator.Send(new CreateCompetitionCommand()
        {
            Name = (command.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value as string)!,
            Description = command.Data.Options.FirstOrDefault(o => o.Name == "description")?.Value as string,

            StartDate = command.Data.Options.FirstOrDefault(o => o.Name == "start_date")?.Value is string sdv
                ? DateTime.Parse(sdv, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
                : DateTime.UtcNow,
            EndDate = command.Data.Options.FirstOrDefault(o => o.Name == "end_date")?.Value is string edv
                ? DateTime.Parse(edv, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
                : default
        });

        await command.FollowupAsync($"Competition successfully created. Id: {resp.Id}.");
    }
}