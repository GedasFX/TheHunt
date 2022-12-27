namespace TheHunt.Application.Helpers;

public record UserContext(ulong UserId);

public interface IRequestContextAccessor
{
    public UserContext Context { get; set; }
}

public class RequestContextAccessor : IRequestContextAccessor
{
    private static UserContext DefaultContext { get; } = new(0);
    public UserContext Context { get; set; } = DefaultContext;
}