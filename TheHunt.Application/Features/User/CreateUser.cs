using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHunt.Domain;

namespace TheHunt.Application.Features.User;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, RegisterUserResponse>
{
    private readonly AppDbContext _dbContext;

    public CreateUserCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegisterUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username, cancellationToken: cancellationToken))
            throw new EntityValidationException("This username is already taken!");

        var salt = RandomNumberGenerator.GetBytes(8);
        var password = Rfc2898DeriveBytes.Pbkdf2(request.Password, salt, 1000, HashAlgorithmName.SHA256, 2048);

        var user = new Domain.Models.User
        {
            Username = request.Username,
            PasswordSalt = salt, PasswordHash = password,
        };

        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse();
    }
}