﻿using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using TheHunt.Domain;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("show", "Provides the competition overview.")]
    public class CompetitionsShowModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _dbContext;

        public CompetitionsShowModule(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [SlashCommand("overview", "Provides a high level overview of competition.")]
        public async Task Overview(
            [Summary(description: "Channel of competition. Defaults to current channel.")]
            ITextChannel? channel = null)
        {
            // var competition = await _dbContext.Competitions.AsNoTracking()
            //     .Where(c => c.ChannelId == (channel != null ? channel.Id : Context.Channel.Id))
            //     .Select(c => new
            //     {
            //         c.Name, c.Description, c.StartDate, c.EndDate,
            //         TotalMembers = c.Members!.Count(),
            //         TotalSubmissions = 0,
            //         Verifiers = c.Members!.Where(m => m.IsModerator).Select(m => new { m.UserId })
            //     }).SingleOrDefaultAsync();
            //
            // if (competition == null)
            // {
            //     await RespondAsync(embed: new EmbedBuilder().WithDescription("Requested competition does not exist.").Build(), ephemeral: true);
            //     return;
            // }
            //
            // string GetVerifiers()
            // {
            //     var verifiers = string.Join(", ", competition.Verifiers.Select(s => $"<@{s.UserId}>"));
            //     return !string.IsNullOrEmpty(verifiers) ? verifiers : "N/A";
            // }
            //
            // await RespondAsync(
            //     embed: new EmbedBuilder()
            //         .WithTitle(competition.Name)
            //         .WithDescription(competition.Description)
            //         .AddField("Start Date", $"<t:{(int)(competition.StartDate - DateTime.UnixEpoch).TotalSeconds}:F>")
            //         .AddField("End Date", competition.EndDate != null ? $"<t:{(int)(competition.EndDate.Value - DateTime.UnixEpoch).TotalSeconds}:F>" : "N/A")
            //         .AddField("Total Members", competition.TotalMembers, inline: true)
            //         .AddField("Total Submissions", competition.TotalSubmissions, inline: true)
            //         .AddField("Verifiers", GetVerifiers())
            //         .WithColor(0xA44200)
            //         .Build(),
            //     ephemeral: true);
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
    }
}