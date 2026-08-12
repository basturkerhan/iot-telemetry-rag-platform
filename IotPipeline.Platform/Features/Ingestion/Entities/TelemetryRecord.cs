using Pgvector;
using System.ComponentModel.DataAnnotations.Schema;

namespace IotPipeline.Platform.Features.Ingestion.Entities;

public class TelemetryRecord
{
    public Guid Id { get; init; }
    public string DeviceId { get; init; } = null!;
    public double Temperature { get; init; }
    public double Humidity { get; init; }
    public double Vibration { get; init; }
    public DateTime Timestamp { get; init; }
    public string SummaryText { get; init; } = null!;

    [Column(TypeName = "vector(384)")]
    public Vector? Embedding { get; set; }

    private TelemetryRecord() { }

    public static TelemetryRecord Create(
        string deviceId,
        double temperature,
        double humidity,
        double vibration,
        DateTime timestamp,
        string summaryText,
        Vector? embedding = null
    )
    {
        var record = new TelemetryRecord
        {
            Id = Guid.CreateVersion7(),
            DeviceId = deviceId,
            Temperature = temperature,
            Humidity = humidity,
            Vibration = vibration,
            Timestamp = timestamp,
            SummaryText = summaryText,
            Embedding = embedding
        };

        return record;
    }
}
