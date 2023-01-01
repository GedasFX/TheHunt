using Discord;
using Discord.Interactions;
using TheHunt.Application;
using TheHunt.Bot.Utils;
using TheHunt.Data.Services;
using TheHunt.Sheets.Services;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("members", "Provides tools for managing competition members.")]
    public class CompetitionsMembersModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SpreadsheetService _sheet;
        private readonly SpreadsheetQueryService _queryService;
        private readonly CompetitionsQueryService _competitionsQueryService;

        public CompetitionsMembersModule(SpreadsheetService sheet, SpreadsheetQueryService queryService, CompetitionsQueryService competitionsQueryService)
        {
            _sheet = sheet;
            _queryService = queryService;
            _competitionsQueryService = competitionsQueryService;
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("invite", "Adds a user to the competition.")]
        public async Task Invite(
            [Summary(description: "User to invite to the competition.")]
            IGuildUser user,
            [Summary(description: "Team name. If left unspecified, will not assign a team to the participant.")]
            string? team = null)
        {
            await DeferAsync(ephemeral: true);

            if (await _queryService.GetCompetitionMember(Context.Channel.Id, user.Id) != null)
                throw new EntityValidationException($"<@{user.Id}> is already part of the competition.");

            var sheetRef = (await _competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!;

            await _sheet.AddMember(sheetRef, user.Id, user.DisplayName, team);
            _queryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"<@{user.Id}> was successfully added to the competition.", ephemeral: true,
                components: new ComponentBuilder().AddRow(new ActionRowBuilder()
                    .WithSpreadsheetRefButton("Open Google Sheets", "📑", sheetRef.SpreadsheetId, sheetRef.Sheets.Members)).Build());
            await FollowupAsync($"<@{Context.User.Id}> added <@{user.Id}> to the competition.");
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("kick", "Removes a user from the competition.")]
        public async Task Kick(
            [Summary(description: "User to remove from the competition.")]
            IGuildUser user)
        {
            await DeferAsync(ephemeral: true);

            var participant = await _queryService.GetCompetitionMember(Context.Channel.Id, user.Id);
            if (participant == null)
                throw new EntityValidationException($"<@{user.Id}> is not part of the competition.");

            var sheetRef = (await _competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!;

            await _sheet.RemoveMember(sheetRef, participant.RowIdx);
            _queryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"<@{user.Id}> was successfully removed from the competition.", ephemeral: true,
                components: new ComponentBuilder().AddRow(new ActionRowBuilder()
                    .WithSpreadsheetRefButton("Open Google Sheets", "📑", sheetRef.SpreadsheetId, sheetRef.Sheets.Members)).Build());
            await FollowupAsync($"<@{Context.User.Id}> removed <@{user.Id}> from the competition.");
        }
    }
}