using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Domain;

namespace TheHunt.Application.Features.Competition;

public class GetCompetitionQueryHandler :
    IRequestHandler<ListCompetitionsQuery, ListCompetitionsResponse>,
    IRequestHandler<GetCompetitionQuery, GetCompetitionResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IHashids _hashids;

    public GetCompetitionQueryHandler(AppDbContext dbContext, IHashids hashids)
    {
        _dbContext = dbContext;
        _hashids = hashids;
    }

    public async Task<GetCompetitionResponse> Handle(GetCompetitionQuery request, CancellationToken cancellationToken)
    {
        var id = _hashids.DecodeSingleLong(request.Id);
        var competition = await MapToDto(_dbContext.Competitions
                .Where(c => c.Id == id))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (competition == null)
            throw new EntityNotFoundException("Competition not found.");

        return new GetCompetitionResponse { Item = competition };
    }

    public async Task<ListCompetitionsResponse> Handle(ListCompetitionsQuery request, CancellationToken cancellationToken)
    {
        var competitions = await MapToDto(_dbContext.Competitions
                // .Where(e => e.IsListed)
                .OrderBy(e => e.Id)
                .Page(request.Page.Index, request.Page.Size))
            .ToListAsync(cancellationToken: cancellationToken);

        return new ListCompetitionsResponse { Items = { competitions } };
    }

    private IQueryable<CompetitionDto> MapToDto(IQueryable<Domain.Models.Competition> competitions) =>
        competitions.Select(c => new CompetitionDto
        {
            Id = _hashids.EncodeLong(c.Id),
            Name = c.Name,
            Description = c.Description,
            StartDate = Timestamp.FromDateTime(c.StartDate),
            EndDate = Timestamp.FromDateTime(c.EndDate),
        }).AsNoTracking();
}