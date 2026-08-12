using IotPipeline.Platform.Features.API.Shared.Messaging;

namespace IotPipeline.Platform.Features.API.Analytics.AskAI;

public record AskAIQuery(
    string Question
)
: IApplicationRequest<AskAIQueryResponseDto>;