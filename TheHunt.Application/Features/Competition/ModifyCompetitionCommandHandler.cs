using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application.Helpers;
using TheHunt.Domain;
using TheHunt.Domain.Models;

namespace TheHunt.Application.Features.Competition;

public record CreateCompetitionCommand : IRequest<CreateCompetitionResponse>
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public class ModifyCompetitionCommandHandler :
    IRequestHandler<CreateCompetitionCommand, CreateCompetitionResponse>//,
    // IRequestHandler<UpdateCompetitionCommand, UpdateCompetitionResponse>,
    // IRequestHandler<DeleteCompetitionCommand, DeleteCompetitionResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IRequestContextAccessor _requestContextAccessor;

    public ModifyCompetitionCommandHandler(AppDbContext dbContext, IRequestContextAccessor requestContextAccessor)
    {
        _dbContext = dbContext;
        _requestContextAccessor = requestContextAccessor;
    }

    public async Task<CreateCompetitionResponse> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Models.Competition
        {
            Name = request.Name, Description = request.Description,
            StartDate = request.StartDate, EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            Members = new CompetitionUser[]
            {
                new() { UserId = _requestContextAccessor.Context.UserId, IsAdmin = true, IsModerator = true, RegistrationDate = DateTime.UtcNow }
            },
        };

        _dbContext.Competitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCompetitionResponse() { Id = entity.Id.ToUserId() };
    }

    public async Task<UpdateCompetitionResponse> Handle(UpdateCompetitionCommand request, CancellationToken cancellationToken)
    {
        if (request.Item.EndDate <= request.Item.StartDate)
            throw new EntityValidationException("End date cannot be be before or equal to start date.");

        var currentDate = DateTime.UtcNow;
        var startDate = request.Item.StartDate.ToDateTime();
        var endDate = request.Item.EndDate.ToDateTime();

        if (startDate < currentDate || endDate < currentDate)
            throw new EntityValidationException("Cannot update events which would make them start or end in the past.");

        var entity = await _dbContext.Competitions
            .Where(e => e.Id == request.Id.ToInternalId())
            .Where(e => e.Members!.Any(m => m.UserId == _requestContextAccessor.Context.UserId && m.IsAdmin))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (entity == null)
            throw new EntityNotFoundException("Competition not found.");


        if (entity.StartDate < currentDate)
            throw new EntityValidationException("Cannot edit start date of already started events.");
        if (entity.EndDate < currentDate)
            throw new EntityValidationException("Cannot edit end date of already finished events.");


        entity.Name = request.Item.Name;
        entity.Description = request.Item.Description;

        entity.StartDate = startDate;
        entity.EndDate = endDate;
        entity.UpdatedAt = currentDate;

        entity.IsListed = request.Item.IsListed;


        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCompetitionResponse();
    }

    public async Task<DeleteCompetitionResponse> Handle(DeleteCompetitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Competitions
            .Where(e => e.Id == request.Id.ToInternalId())
            .Where(e => e.Members!.Any(m => m.UserId == _requestContextAccessor.Context.UserId && m.IsAdmin))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (entity == null)
            throw new EntityNotFoundException("Competition not found.");

        _dbContext.Competitions.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteCompetitionResponse();
    }
}

public class CreateCompetitionCommandValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionCommandValidator()
    {
        RuleFor(r => r.Name).NotEmpty();

        RuleFor(r => r.StartDate).NotEmpty();
        RuleFor(r => r.EndDate).NotEmpty().GreaterThan(e => e.StartDate);
    }
}