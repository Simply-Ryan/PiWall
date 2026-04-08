using Xunit;
using PitWall.Telemetry.Models;
using PitWall.Telemetry.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace PitWall.Telemetry.Tests.Unit;

public class TelemetryNormalizerTests
{
    private readonly TelemetryNormalizer _normalizer;

    public TelemetryNormalizerTests()
    {
        var mockLogger = new Mock<ILogger<TelemetryNormalizer>>();
        _normalizer = new TelemetryNormalizer(mockLogger.Object);
    }

    [Fact]
    public void ValidateTelemetry_WithValidData_ReturnsTrue()
    {
        // Arrange
        var telemetry = CreateValidTelemetryData();

        // Act
        var result = _normalizer.ValidateTelemetry(telemetry);

        // Assert
        Assert.True(result);
        Assert.Empty(_normalizer.GetValidationErrors());
    }

    [Fact]
    public void ValidateTelemetry_WithNullData_ReturnsFalse()
    {
        // Act
        var result = _normalizer.ValidateTelemetry(null!);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(_normalizer.GetValidationErrors());
    }

    [Fact]
    public void ValidateTelemetry_WithInvalidTimestamp_ReturnsFalse()
    {
        // Arrange
        var telemetry = CreateValidTelemetryData();
        telemetry.Timestamp = -1;

        // Act
        var result = _normalizer.ValidateTelemetry(telemetry);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTelemetry_WithInvalidSpeed_ReturnsFalse()
    {
        // Arrange
        var telemetry = CreateValidTelemetryData();
        telemetry.Vehicle.SpeedKmh = 500; // Out of range

        // Act
        var result = _normalizer.ValidateTelemetry(telemetry);

        // Assert
        Assert.False(result);
    }

    private static UnifiedTelemetryData CreateValidTelemetryData()
    {
        return new UnifiedTelemetryData
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Session = new SessionData
            {
                Id = "session-123",
                Game = "iRacing",
                Track = "Monza",
                Type = "r",
                DurationSeconds = 3600,
            },
            Vehicle = new VehicleData
            {
                SpeedKmh = 250f,
                Gear = 4,
                Rpm = 8500,
                FuelLiters = 50f,
                FuelConsumedThisLap = 1.5f,
                DistanceThisLapMeters = 3000f,
            },
            Inputs = new InputData
            {
                Throttle = 0.9f,
                Brake = 0f,
                SteeringAngle = 0.1f,
            },
            Performance = new PerformanceData
            {
                LapNumber = 5,
                DeltaToBestLap = -0.125f,
                DeltaToSessionAverage = -0.050f,
                CurrentLapTime = 125.5f,
            },
        };
    }
}
