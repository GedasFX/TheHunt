using Discord;
using Discord.Interactions;
using MediatR;
using TheHunt.Application.Features.Competition;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Bot.Modules;

[Group("competitions", "Tools to manage competitions.")]
public partial class CompetitionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;
    private readonly IRequestContextAccessor _contextAccessor;
    private readonly AppDbContext _dbContext;

    public CompetitionsModule(IMediator mediator, IRequestContextAccessor contextAccessor, AppDbContext dbContext)
    {
        _mediator = mediator;
        _contextAccessor = contextAccessor;
        _dbContext = dbContext;
    }

    public override void BeforeExecute(ICommandInfo command)
    {
        _contextAccessor.Context = new UserContext(Context.User.Id);
        base.BeforeExecute(command);
    }

    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [SlashCommand("create", "Creates a new competition and binds it to the channel.")]
    public async Task Create(
        [Summary(description: "Name of the competition. Defaults to specified channel name.")]
        string? name = null,
        [Summary(description: "Short Description about the competition.")]
        string? description = null,
        [Summary(description: "Start date for the competition (in UTC). Example: '2022-12-26 21:30:00'. Default: now.")]
        DateTime? startDate = default,
        [Summary(description: "End date for the competition (in UTC). Example: '2023-12-26 21:30:00'. Default: never.")]
        DateTime? endDate = default)
    {
        await Task.WhenAll(DeferAsync(ephemeral: true),
            _mediator.Send(new CreateCompetitionCommand
            {
                Name = name ?? Context.Channel.Name,
                Description = description,
                ChannelId = Context.Channel.Id,
                StartDate = startDate ?? DateTime.UtcNow, EndDate = endDate,
            }));
        await FollowupAsync("Competition successfully created. You can view it with /competitions show");
    }
}