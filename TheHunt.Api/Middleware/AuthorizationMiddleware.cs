using Google.Protobuf;
using Microsoft.AspNetCore.DataProtection;
using TheHunt.Application.Helpers;
using TheHunt.Domain;

namespace TheHunt.Api.Middleware;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDataProtector _protector;

    public AuthorizationMiddleware(RequestDelegate next, IDataProtectionProvider dataProtectionProvider)
    {
        _next = next;
        _protector = dataProtectionProvider.CreateProtector("TheHunt.Api");
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IRequestContextAccessor requestContextAccessor)
    {
        // If none are multiple headers are present - ignore.
        if (context.Request.Headers.Authorization.Count != 1 || !context.Request.Headers.Authorization[0]!.StartsWith("Bearer"))
        {
            await _next(context); return;
        }

        var user = new UserData();
        user.MergeFrom(_protector.Unprotect(Convert.FromBase64String(context.Request.Headers.Authorization[0]![6..])));
        
        if (DateTime.UtcNow - user.LoginDate.ToDateTime() > TimeSpan.FromDays(30))
        {
            await _next(context); return;
        }

        requestContextAccessor.Context = new UserContext((ulong)user.UserId);

        await _next(context);
    }
}