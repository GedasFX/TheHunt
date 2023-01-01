using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using TheHunt.Core.Exceptions;
using TheHunt.Data;
using TheHunt.Data.Models;
using TheHunt.Sheets.Services;

namespace TheHunt.Bot.Modules;

[Group("competitions", "Tools to manage competitions.")]
public partial class CompetitionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AppDbContext _dbContext;
    private readonly SpreadsheetService _sheet;
    private readonly SpreadsheetQueryService _spreadsheetQueryService;

    public CompetitionsModule(AppDbContext dbContext, SpreadsheetService sheet, SpreadsheetQueryService spreadsheetQueryService)
    {
        _dbContext = dbContext;
        _sheet = sheet;
        _spreadsheetQueryService = spreadsheetQueryService;
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("create", "Creates a new competition and binds it to the channel.")]
    public async Task Create(
        [Summary(description: "Google Spreadsheet Id. 'https://docs.google.com/spreadsheets/d/<SPREADSHEET_ID>/edit#gid=0'")]
        string spreadsheetId,
        [Summary(description: "Users with this role will be able to verify submissions.")]
        IRole verifierRole,
        [Summary(description: "Name of the competition. Defaults to specified channel name.")]
        string? name = null)
    {
        if (spreadsheetId.Length != 44)
        {
            await RespondAsync("Invalid spreadsheet id. Extract this bit from the URL: https://docs.google.com/spreadsheets/d/**<SPREADSHEET_ID>**/edit#gid=0");
            return;
        }

        await DeferAsync(ephemeral: true);

        if (await _dbContext.Competitions.AsNoTracking().AnyAsync(c => c.ChannelId == Context.Channel.Id))
            throw new EntityValidationException("This channel already has a competition. Please use a different channel to create a new competition.");

        var entity = new Competition
        {
            ChannelId = Context.Channel.Id, VerifierRoleId = verifierRole.Id,
            Spreadsheet = await _sheet.CreateCompetition(spreadsheetId, name ?? $"#{Context.Channel.Name}")
        };

        _dbContext.Competitions.Add(entity);
        await _dbContext.SaveChangesAsync();

        await FollowupAsync("Competition successfully created. You can view it with /competitions show");
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("reload", "Reloads the competition data from the spreadsheet.")]
    public async Task Reload()
    {
        _spreadsheetQueryService.InvalidateCache(Context.Channel.Id, "members");
        await RespondAsync("Configuration reloaded.");
    }
}