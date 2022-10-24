using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Domain;

namespace TheHunt.Application.Features.User;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Empty>
{
    private readonly AppDbContext _dbContext;

    public CreateUserCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Empty> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.User.Username, cancellationToken: cancellationToken))
            throw new EntityValidationException("This username is already taken!");

        var user = new Domain.Models.User
        {
            Username = request.User.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.User.Password),
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new Empty();
    }
}