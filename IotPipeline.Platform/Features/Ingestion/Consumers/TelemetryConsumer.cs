using IotPipeline.Platform.Common.Interfaces;
using IotPipeline.Platform.Features.Bridge.Contracts;
using IotPipeline.Platform.Features.Ingestion.Entities;
using IotPipeline.Platform.Infrastructure;
using MassTransit;
using Pgvector;
using System.Diagnostics;

namespace IotPipeline.Platform.Features.Ingestion.Consumers;

public class TelemetryConsumer(
    AppDbContext dbContext,
    ILogger<TelemetryConsumer> logger,
    IEmbeddingService embeddingService,
    IConfiguration configuration) : IConsumer<Batch<TelemetryReceivedEvent>>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<TelemetryConsumer> _logger = logger;
    private readonly IEmbeddingService _embeddingService = embeddingService;
    private readonly ActivitySource _activitySource = new(configuration["OTEL:ServiceName"] ?? "IotPipeline.Platform");

    public async Task Consume(ConsumeContext<Batch<TelemetryReceivedEvent>> context)
    {
        Activity.Current?.SetTag("iot.batch.size", context.Message.Length);
        var records = new List<TelemetryRecord>(context.Message.Length);

        using (var embeddingActivity = _activitySource.StartActivity("GenerateEmbeddingsBatch", ActivityKind.Internal))
        {
            embeddingActivity?.SetTag("embedding.count", context.Message.Length);

            for (int i = 0; i < context.Message.Length; i++)
            {
                var msg = context.Message[i].Message;

                string summaryText = $"Device {msg.DeviceId} reported Temp: {msg.Temperature}°C, Humidity: {msg.Humidity}%, Vibration: {msg.Vibration} mm/s at {msg.Timestamp:u}";

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("telemetry record received [Details: {Details}]", summaryText);
                }

                float[] embeddingVector = _embeddingService.GetEmbedding(summaryText);

                var record = TelemetryRecord.Create(
                    deviceId: msg.DeviceId,
                    temperature: msg.Temperature,
                    humidity: msg.Humidity,
                    vibration: msg.Vibration,
                    timestamp: msg.Timestamp,
                    summaryText: summaryText,
                    embedding: new Vector(embeddingVector)
                );

                records.Add(record);
            }
        }

        // 2. Veritabanına toplu yazma süresini ölçen tek bir alt span (EF Core altındaki SQL sorgularını da kapsar)
        using (var dbActivity = _activitySource.StartActivity("SaveTelemetryBatchToDatabase", ActivityKind.Internal))
        {
            dbActivity?.SetTag("db.record.count", records.Count);
            await _dbContext.TelemetryRecords.AddRangeAsync(records);
            await _dbContext.SaveChangesAsync();
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{Count} telemetry record added successfully. [TraceId: {TraceId}]",
                records.Count, Activity.Current?.TraceId);
        }
    }
}