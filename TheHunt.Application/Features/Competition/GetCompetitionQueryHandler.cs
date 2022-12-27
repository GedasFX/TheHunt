using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Application.Features.Competition;

public record GetCompetitionQuery(ulong ChannelId) : IRequest<Domain.Models.Competition>;

public class GetCompetitionQueryHandler :
    // IRequestHandler<ListCompetitionsQuery, ListCompetitionsResponse>,
    IRequestHandler<GetCompetitionQuery, Domain.Models.Competition>
{
    private readonly AppDbContext _dbContext;

    public GetCompetitionQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListCompetitionsResponse> Handle(ListCompetitionsQuery request, CancellationToken cancellationToken)
    {
        // var competitions = await MapToDto(_dbContext.Competitions
        //         .Where(e => e.IsListed)
        //         .OrderBy(e => e.Id)
        //         .Page(request.Page.Index, request.Page.Size))
        //     .ToListAsync(cancellationToken: cancellationToken);

        return new ListCompetitionsResponse { };
    }

    public async Task<Domain.Models.Competition> Handle(GetCompetitionQuery request, CancellationToken cancellationToken)
    {
        var competition = await _dbContext.Competitions.AsNoTracking()
            .Where(q => q.ChannelId == request.ChannelId)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (competition == null)
            throw new EntityNotFoundException("Competition not found.");

        return competition;
    }

    private static IQueryable<CompetitionDto> MapToDto(IQueryable<Domain.Models.Competition> competitions) =>
        competitions.Select(c => new CompetitionDto
        {
            // Id = c.Id.ToUserId(),
            Name = c.Name, Description = c.Description,
            StartDate = Timestamp.FromDateTime(c.StartDate),
            // EndDate = Timestamp.FromDateTime(c.EndDate),
        }).AsNoTracking();
}