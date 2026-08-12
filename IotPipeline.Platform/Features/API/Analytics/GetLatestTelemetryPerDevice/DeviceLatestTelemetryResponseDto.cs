namespace IotPipeline.Platform.Features.API.Analytics.GetLatestTelemetryPerDevice;

public record DeviceLatestTelemetryDto(
    string DeviceId,
    double Temperature,
    double Humidity,
    double Vibration,
    DateTime Timestamp
);
