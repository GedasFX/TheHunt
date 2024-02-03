using Discord;
using Discord.Interactions;
using TheHunt.Core.Exceptions;
using TheHunt.Data;
using TheHunt.Data.Services;
using TheHunt.Sheets.Services;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("config", "Manage competition configuration. To see active config, run /competitions show-config.")]
    public class CompetitionsConfigModule(
        AppDbContext dbContext)
        : InteractionModuleBase<SocketInteractionContext>
    {
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("verifier-role", "Changes the verifier role for the competition.")]
        public async Task VerifierRole(
            [Summary(description: "Users with this role will be able to verify submissions.")]
            IRole newRole)
        {
            var competition = await dbContext.Competitions.FindAsync(Context.Channel.Id) ??
                              throw EntityNotFoundException.CompetitionNotFound;

            var previousVerifierRoleId = competition.VerifierRoleId;

            competition.VerifierRoleId = newRole.Id;
            await dbContext.SaveChangesAsync();

            await RespondAsync(
                $"Verifier role was updated from {MentionUtils.MentionRole(previousVerifierRoleId)} to {MentionUtils.MentionRole(competition.VerifierRoleId)}.",
                ephemeral: true);
        }
    }
}