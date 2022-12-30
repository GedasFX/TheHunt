using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using TheHunt.Bot.Services;
using TheHunt.Domain;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("show", "Provides the competition overview.")]
    public class CompetitionsShowModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _dbContext;
        private readonly SpreadsheetQueryService _spreadsheetQueryService;

        public CompetitionsShowModule(AppDbContext dbContext, SpreadsheetQueryService spreadsheetQueryService)
        {
            _dbContext = dbContext;
            _spreadsheetQueryService = spreadsheetQueryService;
        }

        [SlashCommand("overview", "Provides a high level overview of competition.")]
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
            var submissionsCount = 0;

            string GetVerifiers()
            {
                var v = string.Join(", ", verifiers.Select(s => $"<@{s.Key}>"));
                return !string.IsNullOrEmpty(v) ? v : "N/A";
            }

            await RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(Context.Channel.Name)
                    .WithUrl(GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.OverviewSheetId))
                    .AddField("Submissions Channel", $"<#{competition.SubmissionChannelId}>")
                    .AddField("Start Date", $"<t:{(int)(competition.StartDate - DateTime.UnixEpoch).TotalSeconds}:F>")
                    .AddField("End Date", competition.EndDate != null ? $"<t:{(int)(competition.EndDate.Value - DateTime.UnixEpoch).TotalSeconds}:F>" : "N/A")
                    .AddField("Total Members", membersCount, inline: true).AddField("Total Submissions", submissionsCount, inline: true)
                    .AddField("Verifiers", GetVerifiers())
                    .WithColor(0xA44200)
                    .Build(),
                components: new ComponentBuilder()
                    .AddRow(new ActionRowBuilder()
                        .WithButton(label: "Overview", emote: new Emoji("📖"), style: ButtonStyle.Link,
                            url: GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.OverviewSheetId))
                        .WithButton(label: "Members", emote: new Emoji("🤝"), style: ButtonStyle.Link,
                            url: GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.MembersSheetId))
                        .WithButton(label: "Submissions", emote: new Emoji("🖼️"), style: ButtonStyle.Link,
                            url: GetSheetUrl(competition.Spreadsheet.SpreadsheetId, competition.Spreadsheet.SubmissionsSheetId)))
                    .Build(),
                ephemeral: !@public);
        }

        [SlashCommand("members", "Lists members of the competition.")]
        public async Task Members(
            [Summary(description: "Channel of competition. Defaults to current channel.")]
            ITextChannel? channel = null,
            [Summary(description: "Second page of Google. First page is index 0.")]
            int page = 0)
        {
            // var competition = await _dbContext.Competitions.AsNoTracking()
            //     .Where(c => c.ChannelId == (channel != null ? channel.Id : Context.Channel.Id))
            //     .Select(c => new
            //     {
            //         c.Name, TotalMembers = c.Members!.Count(),
            //         Members = c.Members!.OrderBy(m => m.RegistrationDate).Skip(page * 25).Take(25).Select(m => new { m.UserId, m.IsModerator }),
            //     }).SingleOrDefaultAsync();
            //
            // if (competition == null)
            // {
            //     await RespondAsync(embed: new EmbedBuilder().WithDescription("Requested competition does not exist.").Build(), ephemeral: true);
            //     return;
            // }
            //
            // string GetMembers()
            // {
            //     var members = string.Join(Environment.NewLine,
            //         competition.Members.Select((m, i) => $"`{i + page * 25 + 1:000}` {(m.IsModerator ? "🕵️ " : "")}<@{m.UserId}>"));
            //     return !string.IsNullOrEmpty(members) ? members : "N/A";
            // }
            //
            // await RespondAsync(
            //     embed: new EmbedBuilder()
            //         .WithTitle(competition.Name)
            //         .AddField("Total Members", competition.TotalMembers, inline: true)
            //         .AddField("Page Index", page, inline: true)
            //         .AddField("Members", GetMembers())
            //         .WithColor(0xA44200)
            //         .Build(),
            //     ephemeral: true);
        }

        private static string GetSheetUrl(string spreadsheetId, int sheetId)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={sheetId}";
        }
    }
}