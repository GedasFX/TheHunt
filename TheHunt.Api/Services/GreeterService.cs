using Grpc.Core;
using MediatR;

namespace TheHunt.Api.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly IMediator _mediator;

    public GreeterService(ILogger<GreeterService> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return _mediator.Send(request);
    }

    public override Task<HelloReply> SayHello2(HelloRequest request, ServerCallContext context)
    {
        return _mediator.Send(request);
    }

    public override Task<GetStateResponse> GetState(GetStateQuery request, ServerCallContext context)
    {
        return _mediator.Send(request);
    }
}
