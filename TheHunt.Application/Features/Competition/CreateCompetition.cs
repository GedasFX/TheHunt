using System;
using System.Threading;
using System.Threading.Tasks;
using HashidsNet;
using MediatR;
using TheHunt.Application.Helpers;
using TheHunt.Domain;
using TheHunt.Domain.Models;

namespace TheHunt.Application.Features.Competition;

public class CreateCompetitionCommandHandler : IRequestHandler<CreateCompetitionCommand, CreateCompetitionResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IHashids _hashids;
    private readonly IRequestContextAccessor _requestContextAccessor;

    public CreateCompetitionCommandHandler(AppDbContext dbContext, IHashids hashids, IRequestContextAccessor requestContextAccessor)
    {
        _dbContext = dbContext;
        _hashids = hashids;
        _requestContextAccessor = requestContextAccessor;
    }

    public async Task<CreateCompetitionResponse> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        if (request.EndDate <= request.StartDate)
            throw new EntityValidationException("End date cannot be be before or equal to start date.");

        var entity = new Domain.Models.Competition
        {
            Name = request.Name, Description = request.Description,
            StartDate = request.StartDate.ToDateTime(), EndDate = request.EndDate.ToDateTime(),
            Members = new CompetitionUser[]
            {
                new() { UserId = _requestContextAccessor.Context.UserId, IsAdmin = true, IsModerator = true, RegistrationDate = DateTime.UtcNow }
            },
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Competitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCompetitionResponse() { Id = _hashids.EncodeLong(entity.Id) };
    }
}