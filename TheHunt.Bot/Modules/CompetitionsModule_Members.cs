using Discord;
using Discord.Interactions;
using TheHunt.Application;
using TheHunt.Bot.Internal;
using TheHunt.Bot.Services;

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
            [Summary(description: "Role. Defaults to 'Member'")] [Choice("🔍 Verifier", 1), Choice("Member", 0)]
            int role = 0)
        {
            await DeferAsync(ephemeral: true);

            if (await _queryService.GetCompetitionMember(Context.Channel.Id, user.Id) != null)
                throw new EntityValidationException($"<@{user.Id}> is already part of the competition.");

            await _sheet.AddMember((await _competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!, user.Id, user.DisplayName, role, null);
            _queryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"<@{user.Id}> was successfully added to the competition.", ephemeral: true);
            await FollowupAsync($"<@{Context.User.Id}> added <@{user.Id}> to the competition.");
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("kick", "Removes a user from the competition.")]
        public async Task Kick(
            [Summary(description: "User to remove from the competition.")]
            IGuildUser user)
        {
            await DeferAsync(ephemeral: true);

            var competitor = await _queryService.GetCompetitionMember(Context.Channel.Id, user.Id);
            if (competitor == null)
                throw new EntityValidationException($"<@{user.Id}> is not part of the competition.");

            await _sheet.RemoveMember((await _competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!, competitor.RowIdx);
            _queryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"<@{user.Id}> was successfully removed from the competition.", ephemeral: true);
            await FollowupAsync($"<@{Context.User.Id}> removed <@{user.Id}> from the competition.");
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("set-role", "Changes the permissions of the user.")]
        public async Task SetRole(
            [Summary(description: "User to remove from the competition.")]
            IGuildUser user,
            [Summary(description: "Role. Defaults to 'Member'")] [Choice("Verifier", 1), Choice("Member", 0)]
            int role)
        {
            await DeferAsync(ephemeral: true);

            var competitor = await _queryService.GetCompetitionMember(Context.Channel.Id, user.Id);
            if (competitor == null)
                throw new EntityValidationException($"<@{user.Id}> is not part of the competition.");

            await _sheet.UpdateMemberRole((await _competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id))!, competitor.RowIdx, role);
            _queryService.InvalidateCache(Context.Channel.Id, "members");

            await FollowupAsync($"<@{user.Id}> is now a {(role == 1 ? "Verifier" : "Regular participant")}.", ephemeral: true);
            await FollowupAsync($"<@{Context.User.Id}> made <@{user.Id}> a {(role == 1 ? "Verifier" : "Regular participant")}.");
        }
    }
}