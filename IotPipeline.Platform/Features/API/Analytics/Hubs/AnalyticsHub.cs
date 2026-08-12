using IotPipeline.Platform.Common.Results;
using IotPipeline.Platform.Features.API.Analytics.AskAI;
using IotPipeline.Platform.Features.API.Analytics.GetLatestTelemetryPerDevice;
using MassTransit.Mediator;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace IotPipeline.Platform.Features.API.Analytics.Hubs;

public class AnalyticsHub(IMediator mediator) : Hub
{
    public async Task AskAIAsync(string input)
    {
        AskAIQuery query = new(input);
        var client = mediator.CreateRequestClient<AskAIQuery>();
        var response = await client.GetResponse<Result<AskAIQueryResponseDto>>(query);
        await Clients.Caller.SendAsync("ReceiveMessage", response);
    }

    public async IAsyncEnumerable<Result<List<DeviceLatestTelemetryDto>>> StreamLatestTelemetryAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var client = mediator.CreateRequestClient<GetLatestTelemetryPerDeviceQuery>();
        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await client.GetResponse<Result<List<DeviceLatestTelemetryDto>>>(
                new GetLatestTelemetryPerDeviceQuery(),
                cancellationToken
            );

            yield return response.Message;

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }
    }
}
