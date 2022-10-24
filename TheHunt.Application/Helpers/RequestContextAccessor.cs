namespace TheHunt.Application.Helpers;

public interface IRequestContextAccessor
{
    public UserData Context { get; set; }
}

public class RequestContextAccessor : IRequestContextAccessor
{
    public UserData Context { get; set; } = new();
}