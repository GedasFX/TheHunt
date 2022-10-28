using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Application.Features.Competition;

public class CreateCompetitionCommandHandler : IRequestHandler<CreateCompetitionCommand, CreateCompetitionResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IRequestContextAccessor _requestContextAccessor;

    public CreateCompetitionCommandHandler(AppDbContext dbContext, IRequestContextAccessor requestContextAccessor)
    {
        _dbContext = dbContext;
        _requestContextAccessor = requestContextAccessor;
    }

    public async Task<CreateCompetitionResponse> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Models.Competition()
        {
            Name = request.Name, Description = request.Description,
            Admin = new Domain.Models.User() { Id = new Guid(_requestContextAccessor.Context.UserId.Span) },
            
        };
        
        _dbContext.Competitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateCompetitionResponse() { Id = GuidUtils.ToString(entity.Id) };
    }
}