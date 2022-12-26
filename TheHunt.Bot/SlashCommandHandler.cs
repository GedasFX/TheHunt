using System.Globalization;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Application.Features.Competition;
using TheHunt.Application.Helpers;

namespace TheHunt.Bot;

public class SlashCommandHandler
{
    private readonly IServiceProvider _serviceProvider;

    private SlashCommandHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static SlashCommandHandler Register(DiscordSocketClient client, IServiceProvider serviceProvider)
    {
        var handler = new SlashCommandHandler(serviceProvider);
        client.SlashCommandExecuted += async command =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<IRequestContextAccessor>().Context = new UserData()
            {
                Username = command.User.Username,
                UserId = (long)command.User.Id,
            };

            await OnSlashCommandExecuted(scope.ServiceProvider.GetRequiredService<IMediator>(), command);
        };

        return handler;
    }

    private static Task OnSlashCommandExecuted(IMediator mediator, SocketSlashCommand command)
    {
        return command.CommandName switch
        {
            "create" => HandleCreate(mediator, command),
            _ => throw new ArgumentOutOfRangeException(nameof(command), $"Command {command.CommandName} does not have a handler.")
        };
    }

    private static async Task HandleCreate(ISender mediator, SocketSlashCommand command)
    {
        var resp = await mediator.Send(new CreateCompetitionCommand()
        {
            Name = (command.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value as string)!,
            Description = command.Data.Options.FirstOrDefault(o => o.Name == "description")?.Value as string,

            StartDate = command.Data.Options.FirstOrDefault(o => o.Name == "start_date")?.Value is not string sdv
                ? DateTime.UtcNow
                : DateTime.Parse(sdv, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
            EndDate = command.Data.Options.FirstOrDefault(o => o.Name == "end_date")?.Value is not string edv
                ? default
                : DateTime.Parse(edv, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
        });

        await command.RespondAsync($"Competition successfully created. Id: {resp.Id}.");
    }
}