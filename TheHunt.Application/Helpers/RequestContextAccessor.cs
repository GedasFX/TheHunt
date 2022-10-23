using System;

namespace TheHunt.Application.Helpers;

public record RequestContext
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
};

public interface IRequestContextAccessor
{
    public RequestContext Context { get; set; }
}

public class RequestContextAccessor : IRequestContextAccessor
{
    public RequestContext Context { get; set; } = new();
}