namespace IotPipeline.Platform.Features.API.Analytics.AskAI;

public record AskAIQueryResponseDto(
    string Question,
    string Answer,
    int RetrievedContextCount,
    List<string> ContextUsed
);
