using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Application.Features.Competition;

public class GetCompetitionQueryHandler :
    IRequestHandler<ListCompetitionsQuery, ListCompetitionsResponse>,
    IRequestHandler<GetCompetitionQuery, GetCompetitionResponse>
{
    private readonly AppDbContext _dbContext;

    public GetCompetitionQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListCompetitionsResponse> Handle(ListCompetitionsQuery request, CancellationToken cancellationToken)
    {
        var competitions = await MapToDto(_dbContext.Competitions
                .Where(e => e.IsListed)
                .OrderBy(e => e.Id)
                .Page(request.Page.Index, request.Page.Size))
            .ToListAsync(cancellationToken: cancellationToken);

        return new ListCompetitionsResponse { Items = { competitions } };
    }

    public async Task<GetCompetitionResponse> Handle(GetCompetitionQuery request, CancellationToken cancellationToken)
    {
        var id = IdUtils.ToInternalId(request.Id);
        var competition = await MapToDto(_dbContext.Competitions
                .Where(c => c.Id == id))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (competition == null)
            throw new EntityNotFoundException("Competition not found.");

        return new GetCompetitionResponse { Item = competition };
    }

    private static IQueryable<CompetitionDto> MapToDto(IQueryable<Domain.Models.Competition> competitions) =>
        competitions.Select(c => new CompetitionDto
        {
            Id = IdUtils.ToUserId(c.Id),
            Name = c.Name,
            Description = c.Description,
            StartDate = Timestamp.FromDateTime(c.StartDate),
            EndDate = Timestamp.FromDateTime(c.EndDate),
        }).AsNoTracking();
}