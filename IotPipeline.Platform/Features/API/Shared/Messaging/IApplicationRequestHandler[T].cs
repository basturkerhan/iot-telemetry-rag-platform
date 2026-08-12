using IotPipeline.Platform.Common.Results;
using MassTransit;

namespace IotPipeline.Platform.Features.API.Shared.Messaging;

public interface IApplicationRequestHandler<TRequest, TResponse>
        : IConsumer<TRequest>
    where TRequest : class, IApplicationRequest<TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}
