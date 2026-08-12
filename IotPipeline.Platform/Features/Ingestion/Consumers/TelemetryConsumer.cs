using IotPipeline.Platform.Common.Interfaces;
using IotPipeline.Platform.Features.Bridge.Contracts;
using IotPipeline.Platform.Features.Ingestion.Entities;
using IotPipeline.Platform.Infrastructure;
using MassTransit;
using Pgvector;

namespace IotPipeline.Platform.Features.Ingestion.Consumers;

public class TelemetryConsumer(
    AppDbContext dbContext,
    ILogger<TelemetryConsumer> logger,
    IEmbeddingService embeddingService) : IConsumer<Batch<TelemetryReceivedEvent>>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<TelemetryConsumer> _logger = logger;
    private readonly IEmbeddingService _embeddingService = embeddingService;

    public async Task Consume(ConsumeContext<Batch<TelemetryReceivedEvent>> context)
    {
        var records = new List<TelemetryRecord>(context.Message.Length);
        for (int i = 0; i < context.Message.Length; i++)
        {
            var msg = context.Message[i].Message;
            string summaryText = $"Device {msg.DeviceId} reported Temp: {msg.Temperature}°C, Humidity: {msg.Humidity}%, Vibration: {msg.Vibration} mm/s at {msg.Timestamp:u}";

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

        await _dbContext.TelemetryRecords.AddRangeAsync(records);
        await _dbContext.SaveChangesAsync();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{Count} telemetry record added successfully.", records.Count);
        }
    }
}