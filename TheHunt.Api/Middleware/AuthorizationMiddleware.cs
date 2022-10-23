using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Api.Middleware;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IRequestContextAccessor requestContextAccessor)
    {
        // If none are multiple headers are present - ignore.
        if (context.Request.Headers.Authorization.Count != 1 || !context.Request.Headers.Authorization[0]!.StartsWith("Basic"))
        {
            await _next(context);
            return;
        }

        var str = Encoding.ASCII.GetString(Convert.FromBase64String(context.Request.Headers.Authorization[0]![6..])).Split(':');
        var user = await dbContext.Users
            .Where(u => u.Username == str[0])
            .Select(u => new { u.Id, u.PasswordHash, u.PasswordSalt })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            await _next(context);
            return;
        }

        var output = Rfc2898DeriveBytes.Pbkdf2(str[1], user.PasswordSalt, 1000, HashAlgorithmName.SHA256, 2048);
        if (!output.SequenceEqual(user.PasswordHash))
        {
            await _next(context);
            return;
        }

        requestContextAccessor.Context = new RequestContext { UserId = user.Id, Username = str[0] };

        await _next(context);
    }
}