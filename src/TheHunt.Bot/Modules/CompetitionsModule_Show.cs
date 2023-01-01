using Discord;
using Discord.Interactions;
using TheHunt.Bot.Utils;
using TheHunt.Data.Services;
using TheHunt.Sheets.Services;

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

            await DeferAsync(ephemeral: true);

            string GetVerifiers()
            {
                var v = string.Join(", ", Context.Guild.GetRole(competition.VerifierRoleId).Members.Select(s => MentionUtils.MentionUser(s.Id)));
                return !string.IsNullOrEmpty(v) ? v : "N/A";
            }

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle(Context.Channel.Name)
                    .WithUrl(FormatUtils.GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Overview))
                    .WithFields(await _spreadsheetQueryService.GetCompetitionShowFields(competition.Spreadsheet))
                    .AddField("Verifier Role", MentionUtils.MentionRole(competition.VerifierRoleId))
                    .AddField("Verifiers", GetVerifiers())
                    .WithColor(0xA44200)
                    .Build(),
                components: new ComponentBuilder()
                    .AddRow(new ActionRowBuilder()
                        .WithSpreadsheetRefButton("Overview", "📖", competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Overview)
                        .WithSpreadsheetRefButton("Members", "🤝", competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Members)
                        .WithSpreadsheetRefButton("Submissions", "🖼️", competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Submissions))
                    .Build(),
                ephemeral: !@public);
        }
    }
}