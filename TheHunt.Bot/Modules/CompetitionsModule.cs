using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application.Features.Competition;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Bot.Modules;

[Group("competitions", "Tools to manage competitions.")]
public class CompetitionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;
    private readonly IRequestContextAccessor _contextAccessor;

    public CompetitionsModule(IMediator mediator, IRequestContextAccessor contextAccessor)
    {
        _mediator = mediator;
        _contextAccessor = contextAccessor;
    }

    public override void BeforeExecute(ICommandInfo command)
    {
        _contextAccessor.Context = new UserContext(Context.User.Id);
        base.BeforeExecute(command);
    }

    [SlashCommand("create", "Creates a new competition and binds it to the channel.")]
    public async Task Create(
        [Summary(description: "Name of the competition. Defaults to specified channel name.")]
        string? name = null,
        [Summary(description: "Short Description about the competition.")]
        string? description = null,
        [Summary(description: "Channel to bind competition to. Defaults to current channel.")]
        ITextChannel? channel = null,
        [Summary(description: "Start date for the competition (in UTC). Example: '2022-12-26 21:30:00'. Default: now.")]
        DateTime? startDate = default,
        [Summary(description: "End date for the competition (in UTC). Example: '2023-12-26 21:30:00'. Default: never.")]
        DateTime? endDate = default)
    {
        await Task.WhenAll(DeferAsync(ephemeral: true),
            _mediator.Send(new CreateCompetitionCommand
            {
                Name = name ?? channel?.Name ?? Context.Channel.Name,
                Description = description,
                ChannelId = channel?.Id ?? Context.Channel.Id,
                StartDate = startDate ?? DateTime.UtcNow, EndDate = endDate,
            }));
        await FollowupAsync("Competition successfully created. You can view it with /competitions show");
    }

    [Group("show", "Provides an overview of the bound competition.")]
    public class CompetitionsShowModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _dbContext;
        private readonly IRequestContextAccessor _contextAccessor;

        public CompetitionsShowModule(AppDbContext dbContext, IRequestContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
        }

        public override void BeforeExecute(ICommandInfo command)
        {
            _contextAccessor.Context = new UserContext(Context.User.Id);
            base.BeforeExecute(command);
        }

        [SlashCommand("overview", "Provides a high level overview of competition.")]
        public async Task Overview(
            [Summary(description: "Channel to show competition of. Defaults to current channel.")]
            ITextChannel? channel = null)
        {
            var competition = await _dbContext.Competitions.AsNoTracking()
                .Where(c => c.ChannelId == (channel != null ? channel.Id : Context.Channel.Id))
                .Select(c => new
                {
                    c.Name, c.Description, c.StartDate, c.EndDate, c.CreatedAt,
                    TotalMembers = c.Members!.Count(),
                    TotalSubmissions = 0,
                    StaffMembers = c.Members!.Where(m => m.IsAdmin || m.IsModerator).Select(m => new { m.UserId })
                }).SingleOrDefaultAsync();

            if (competition == null)
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("Requested competition does not exist.").Build(), ephemeral: true);
                return;
            }

            await RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(competition.Name)
                    .WithDescription(competition.Description)
                    .AddField("Created At", $"<t:{(int)(competition.CreatedAt - DateTime.UnixEpoch).TotalSeconds}:F>")
                    .AddField("Start Date", $"<t:{(int)(competition.StartDate - DateTime.UnixEpoch).TotalSeconds}:F>")
                    .AddField("End Date", competition.EndDate != null ? $"<t:{(int)(competition.EndDate.Value - DateTime.UnixEpoch).TotalSeconds}:F>" : "N/A")
                    .AddField("Total Members", competition.TotalMembers, inline: true)
                    .AddField("Total Submissions", competition.TotalSubmissions, inline: true)
                    .AddField("Staff", string.Join(Environment.NewLine, competition.StaffMembers.Select(s => $"<@{s.UserId}>")))
                    .WithColor(0xA44200)
                    .Build(),
                ephemeral: true);
        }

        [SlashCommand("members", "Lists members of the competition.")]
        public async Task Members(
            [Summary(description: "Channel to show competition members of. Defaults to current channel.")]
            ITextChannel? channel = null,
            [Summary(description: "Second page of Google. First page is index 0.")]
            int page = 0)
        {
            var competition = await _dbContext.Competitions.AsNoTracking()
                .Where(c => c.ChannelId == (channel != null ? channel.Id : Context.Channel.Id))
                .Select(c => new
                {
                    c.Name, TotalMembers = c.Members!.Count(),
                    Members = c.Members!.OrderBy(m => m.RegistrationDate).Skip(page * 25).Take(25).Select(m => new { m.UserId, m.IsAdmin, m.IsModerator }),
                }).SingleOrDefaultAsync();

            if (competition == null)
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("Requested competition does not exist.").Build(), ephemeral: true);
                return;
            }

            await RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(competition.Name)
                    .AddField("Total Members", competition.TotalMembers)
                    .AddField("Members",
                        string.Join(Environment.NewLine, competition.Members.Select(m => $"{(m.IsAdmin ? "🛠️ " : m.IsModerator ? "🕵️ " : "")}<@{m.UserId}>")))
                    .WithColor(0xA44200)
                    .Build(),
                ephemeral: true);
        }
    }
}