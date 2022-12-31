using Discord;
using Discord.Interactions;
using TheHunt.Bot.Services;
using TheHunt.Bot.Utils;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    public class CompetitionsShowModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SpreadsheetQueryService _spreadsheetQueryService;
        private readonly CompetitionsQueryService _competitionsQueryService;

        public CompetitionsShowModule(SpreadsheetQueryService spreadsheetQueryService, CompetitionsQueryService competitionsQueryService)
        {
            _spreadsheetQueryService = spreadsheetQueryService;
            _competitionsQueryService = competitionsQueryService;
        }

        [SlashCommand("show", "Provides a high level overview of competition.")]
        public async Task Overview(
            [Summary(description: "Should the message be posted to the channel? Defaults to False.")]
            bool @public = false)
        {
            var competition = await _competitionsQueryService.GetCompetition(Context.Channel.Id);
            if (competition == null)
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("Requested competition does not exist.").Build(), ephemeral: true);
                return;
            }

            var membersCount = await _spreadsheetQueryService.GetCompetitionMembersCount(competition.ChannelId);
            const int submissionsCount = 0;

            string GetVerifiers()
            {
                var v = string.Join(", ", Context.Guild.GetRole(competition.VerifierRoleId).Members.Select(s => MentionUtils.MentionUser(s.Id)));
                return !string.IsNullOrEmpty(v) ? v : "N/A";
            }

            await RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(Context.Channel.Name)
                    .WithUrl(FormatUtils.GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Overview))
                    .AddField("Participants Count", membersCount, inline: true).AddField("Submissions Count", submissionsCount, inline: true)
                    .AddField("Verifier Role", MentionUtils.MentionRole(competition.VerifierRoleId))
                    .AddField("Verifiers", GetVerifiers())
                    .WithColor(0xA44200)
                    .Build(),
                components: new ComponentBuilder()
                    .AddRow(new ActionRowBuilder()
                        .WithSpreadsheetRefButton("Overview", "📖", competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Overview)
                        .WithSpreadsheetRefButton("Overview", "🤝", competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Members)
                        .WithSpreadsheetRefButton("Overview", "🖼️", competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Submissions))
                    .Build(),
                ephemeral: !@public);
        }
    }
}