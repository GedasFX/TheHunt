namespace TheHunt.Api.Extensions;

public static class RpcMediatorExtensions
{
    public static IEndpointRouteBuilder MapGrpcMediatorServices(this IEndpointRouteBuilder builder)
    {
        builder.MapGrpcService<Bounty.BountyService>();
        builder.MapGrpcService<User.UserService>();

        return builder;
    }
}