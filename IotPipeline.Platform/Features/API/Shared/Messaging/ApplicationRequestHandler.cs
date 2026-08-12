using IotPipeline.Platform.Common.Results;
using MassTransit;

namespace IotPipeline.Platform.Features.API.Shared.Messaging;

public abstract class ApplicationRequestHandler<TRequest, TResponse>
    : IApplicationRequestHandler<TRequest, TResponse>
    where TRequest : class, IApplicationRequest<TResponse>
{
    public abstract Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);

    public async Task Consume(ConsumeContext<TRequest> context)
    {
        Result<TResponse> result = await Handle(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}
