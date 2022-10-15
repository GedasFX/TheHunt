using Grpc.Core;
using MediatR;
using TheHunt.Pack;

namespace TheHunt.Api.Services;

public class Greeter2Service : Greeter2.Greeter2Base
{
    private readonly IMediator _mediator;

    public Greeter2Service(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public override Task<GetStateResponse1> GetState(GetStateQuery1 request, ServerCallContext context)
    {
        return _mediator.Send(request, context.CancellationToken);
    }
}