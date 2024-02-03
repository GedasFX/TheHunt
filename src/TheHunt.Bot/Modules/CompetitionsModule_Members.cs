using Discord;
using Discord.Interactions;
using TheHunt.Core.Exceptions;
using TheHunt.Data.Services;
using TheHunt.Sheets.Services;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("members", "Provides tools for managing competition members.")]
    public class CompetitionsMembersModule(
        SpreadsheetService sheetService,
        SpreadsheetQueryService sheetQueryService,
        CompetitionsQueryService competitionsQueryService)
        : InteractionModuleBase<SocketInteractionContext>
    {
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("invite", "Adds a user to the competition.")]
        public async Task Invite(
            [Summary(description: "User to invite to the competition.")]
            IGuildUser user,
            [Summary(description: "Team name. If left unspecified, will not assign a team to the participant.")]
            string? team = null)
        {
            await DeferAsync(ephemeral: true);

            if (await sheetQueryService.GetCompetitionMember(Context.Channel.Id, user.Id) != null)
                throw new EntityValidationException(
                    $"{MentionUtils.MentionUser(user.Id)} is already part of the competition.");

            var sheetRef = (await competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!;

            await sheetService.AddMember(sheetRef, user.Id, user.DisplayName, team);
            sheetQueryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"{MentionUtils.MentionUser(user.Id)} was successfully added to the competition.",
                ephemeral: true);
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("kick", "Removes a user from the competition.")]
        public async Task Kick(
            [Summary(description: "User to remove from the competition.")]
            IGuildUser user)
        {
            await DeferAsync(ephemeral: true);

            var participant = await sheetQueryService.GetCompetitionMember(Context.Channel.Id, user.Id);
            if (participant == null)
                throw new EntityValidationException(
                    $"{MentionUtils.MentionUser(user.Id)} is not part of the competition.");

            var sheetRef = (await competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!;

            await sheetService.RemoveMember(sheetRef, participant.RowIdx);
            sheetQueryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"{MentionUtils.MentionUser(user.Id)} was successfully removed from the competition.",
                ephemeral: true);
        }
    }
}