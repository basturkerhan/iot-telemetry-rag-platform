using IotPipeline.Platform.Common.Results;
using IotPipeline.Platform.Features.API.Analytics.AskAI;
using IotPipeline.Platform.Features.API.Shared.Modules;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace IotPipeline.Platform.Features.API.Analytics;

public class AnalyticsEndpoints : BaseModule
{
    protected override string RouteSegment => "analytics";

    protected override void DefineRoutes(RouteGroupBuilder group)
    {
        // OpenAPI/Scalar
        group.WithTags("Analytics");

        group.MapPost("/ask-ai", async (
            [FromBody] AskAIQuery query,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var client = mediator.CreateRequestClient<AskAIQuery>();
            var response = await client.GetResponse<Result<AskAIQueryResponseDto>>(query, cancellationToken);

            if (!response.Message.IsSuccess)
            {
                return Results.BadRequest(response.Message);
            }

            return Results.Ok(response.Message);
        })
        .WithName("AskAI");
    }
}
