using IotPipeline.Platform.Common.Results;
using IotPipeline.Platform.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPipeline.Platform.Features.API.Analytics.GetLatestTelemetryPerDevice;

public class GetLatestTelemetryPerDeviceHandler(
    AppDbContext dbContext,
    ILogger<GetLatestTelemetryPerDeviceHandler> logger) : IConsumer<GetLatestTelemetryPerDeviceQuery>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<GetLatestTelemetryPerDeviceHandler> _logger = logger;

    public async Task Consume(ConsumeContext<GetLatestTelemetryPerDeviceQuery> context)
    {
        var latestRecords = await _dbContext.TelemetryRecords
            .AsNoTracking()
            .GroupBy(r => r.DeviceId)
            .Select(g => g.OrderByDescending(r => r.Timestamp)
                          .Select(r => new DeviceLatestTelemetryDto(
                              r.DeviceId,
                              r.Temperature,
                              r.Humidity,
                              r.Vibration,
                              r.Timestamp
                          ))
                          .FirstOrDefault()!)
            .ToListAsync();

        if (latestRecords.Count == 0)
        {
            await context.RespondAsync(Result<List<DeviceLatestTelemetryDto>>.Failure("Henüz telemetri kaydı bulunamadı."));
            return;
        }

        await context.RespondAsync(Result<List<DeviceLatestTelemetryDto>>.Success(latestRecords));
    }
}
