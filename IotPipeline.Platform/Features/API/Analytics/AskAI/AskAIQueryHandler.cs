using IotPipeline.Platform.Common.Interfaces;
using IotPipeline.Platform.Common.Results;
using IotPipeline.Platform.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace IotPipeline.Platform.Features.API.Analytics.AskAI;

public class AskAIQueryHandler(
        AppDbContext dbContext,
        IRagService ragService,
        IEmbeddingService embeddingService
    )
    : IConsumer<AskAIQuery>
{
    public async Task Consume(ConsumeContext<AskAIQuery> context)
    {
        var request = context.Message;
        var stats = await dbContext.TelemetryRecords
            .AsNoTracking()
            .GroupBy(r => r.DeviceId)
            .Select(g => $"{g.Key}: Ort Temp={Math.Round(g.Average(x => x.Temperature), 1)}°C, Max Temp={g.Max(x => x.Temperature)}°C, Max Vib={g.Max(x => x.Vibration)}")
            .ToListAsync();

        float[] queryVector = embeddingService.GetEmbedding(request.Question);
        var vector = new Vector(queryVector);

        var similarRecords = await dbContext.TelemetryRecords
            .AsNoTracking()
            .Where(r => r.Embedding != null)
            .OrderBy(r => r.Embedding!.CosineDistance(vector))
            .Take(10)
            .Select(r => $"{r.DeviceId} | T:{r.Temperature}°C | H:%{r.Humidity} | V:{r.Vibration} | Time:{r.Timestamp:HH:mm:ss}")
            .ToListAsync();

        if (stats.Count == 0 && similarRecords.Count == 0)
        {
            await context.RespondAsync(Result<AskAIQueryResponseDto>.Failure("Telemetri verisi bulunamadı."));
            return;
        }

        string askContext = $"""
    === DEVICE SUMMARY STATISTICS FOR THE LAST HOUR ===
    {string.Join("\n", stats)}

    === SIMILAR / CRITICAL DROPPED TELEMETRY LOGS ===
    {string.Join("\n", similarRecords)}
    """;

        string aiResponse = await ragService.AskQuestionWithContextAsync(request.Question, askContext);

        AskAIQueryResponseDto responseDto = new(
            Question: request.Question,
            Answer: aiResponse,
            RetrievedContextCount: similarRecords.Count,
            ContextUsed: similarRecords
        );

        await context.RespondAsync(Result<AskAIQueryResponseDto>.Success(responseDto));
    }
}