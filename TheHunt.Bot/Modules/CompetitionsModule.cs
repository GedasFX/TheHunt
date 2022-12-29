using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application;
using TheHunt.Application.Helpers;
using TheHunt.Bot.Internal;
using TheHunt.Domain;
using TheHunt.Domain.Models;

namespace TheHunt.Bot.Modules;

[Group("competitions", "Tools to manage competitions.")]
public partial class CompetitionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;
    private readonly IRequestContextAccessor _contextAccessor;
    private readonly AppDbContext _dbContext;
    private readonly MySheet _sheet;

    public CompetitionsModule(IMediator mediator, IRequestContextAccessor contextAccessor, AppDbContext dbContext, MySheet sheet)
    {
        _mediator = mediator;
        _contextAccessor = contextAccessor;
        _dbContext = dbContext;
        _sheet = sheet;
    }

    public override void BeforeExecute(ICommandInfo command)
    {
        _contextAccessor.Context = new UserContext(Context.User.Id);
        base.BeforeExecute(command);
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("create", "Creates a new competition and binds it to the channel.")]
    public async Task Create(
        [Summary(description: "Google Spreadsheet Id. 'https://docs.google.com/spreadsheets/d/<SPREADSHEET_ID>/edit#gid=0'")]
        string spreadsheetId,
        [Summary(description: "Name of the competition. Defaults to specified channel name.")]
        string? name = null,
        [Summary(description: "Short Description about the competition.")]
        string? description = null,
        [Summary(description: "Submission channel. Defaults to current channel.")]
        ITextChannel? submissionChannel = null,
        [Summary(description: "Start date for the competition (in UTC). Example: '2022-12-26 21:30:00'. Default: now.")]
        DateTime? startDate = default,
        [Summary(description: "End date for the competition (in UTC). Example: '2023-12-26 21:30:00'. Default: never.")]
        DateTime? endDate = default)
    {
        if (spreadsheetId.Length != 44)
        {
            await RespondAsync("Invalid spreadsheet id. Extract this bit from the URL: https://docs.google.com/spreadsheets/d/**<SPREADSHEET_ID>**/edit#gid=0");
            return;
        }

        await DeferAsync(ephemeral: true);

        if (await _dbContext.Competitions.AnyAsync(c => c.ChannelId == Context.Channel.Id))
            throw new EntityValidationException("This channel already has a competition. Please use a different channel to create a new competition.");

        var entity = new Domain.Models.Competition
        {
            ChannelId = Context.Channel.Id, SubmissionChannelId = submissionChannel?.Id ?? Context.Channel.Id,
            Name = name ?? $"#{Context.Channel.Name}", Description = description,
            StartDate = startDate ?? DateTime.UtcNow, EndDate = endDate,
        };

        entity.Spreadsheet = await _sheet.CreateCompetition(entity.Name, spreadsheetId);

        _dbContext.Competitions.Add(entity);
        await _dbContext.SaveChangesAsync();

        await FollowupAsync("Competition successfully created. You can view it with /competitions show");
    }
}