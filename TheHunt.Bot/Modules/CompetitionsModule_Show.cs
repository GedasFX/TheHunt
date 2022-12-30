using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using TheHunt.Bot.Services;
using TheHunt.Domain;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    public class CompetitionsShowModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _dbContext;
        private readonly SpreadsheetQueryService _spreadsheetQueryService;

        public CompetitionsShowModule(AppDbContext dbContext, SpreadsheetQueryService spreadsheetQueryService)
        {
            _dbContext = dbContext;
            _spreadsheetQueryService = spreadsheetQueryService;
        }

        [SlashCommand("show", "Provides a high level overview of competition.")]
        public async Task Overview(
            [Summary(description: "Should the message be posted to the channel? Defaults to False.")]
            bool @public = false)
        {
            var competition = await _dbContext.Competitions.AsNoTracking()
                .Where(c => c.ChannelId == Context.Channel.Id)
                .SingleOrDefaultAsync();

            if (competition == null)
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("Requested competition does not exist.").Build(), ephemeral: true);
                return;
            }

            var membersCount = await _spreadsheetQueryService.GetCompetitionMembersCount(competition.ChannelId);
            var verifiers = await _spreadsheetQueryService.GetCompetitionVerifiers(competition.ChannelId);
            const int submissionsCount = 0;

            string GetVerifiers()
            {
                var v = string.Join(", ", verifiers.Select(s => $"<@{s.Key}>"));
                return !string.IsNullOrEmpty(v) ? v : "N/A";
            }

            await RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(Context.Channel.Name)
                    .WithUrl(GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Overview))
                    // .AddField("Submissions Channel", $"<#{competition.SubmissionChannelId}>")
                    // .AddField("Start Date", $"<t:{(int)(competition.StartDate - DateTime.UnixEpoch).TotalSeconds}:F>")
                    // .AddField("End Date", competition.EndDate != null ? $"<t:{(int)(competition.EndDate.Value - DateTime.UnixEpoch).TotalSeconds}:F>" : "N/A")
                    .AddField("Total Members", membersCount, inline: true).AddField("Total Submissions", submissionsCount, inline: true)
                    .AddField("Verifiers", GetVerifiers())
                    .WithColor(0xA44200)
                    .Build(),
                components: new ComponentBuilder()
                    .AddRow(new ActionRowBuilder()
                        .WithButton(label: "Overview", emote: new Emoji("📖"), style: ButtonStyle.Link,
                            url: GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Overview))
                        .WithButton(label: "Members", emote: new Emoji("🤝"), style: ButtonStyle.Link,
                            url: GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Members))
                        .WithButton(label: "Submissions", emote: new Emoji("🖼️"), style: ButtonStyle.Link,
                            url: GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.Sheets.Submissions)))
                    .Build(),
                ephemeral: !@public);
        }


        private static string GetSheetUrl(string spreadsheetId, int sheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={sheetId}";
        }
    }
}