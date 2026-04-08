using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using PitWall.Telemetry.Models;
using PitWall.Telemetry.Services;

namespace PitWall.Telemetry.Tests.Unit.Services;

public class TelemetryBufferTests
{
    private readonly TelemetryBuffer _buffer;

    public TelemetryBufferTests()
    {
        var mockLogger = new Mock<ILogger<TelemetryBuffer>>();
        _buffer = new TelemetryBuffer(mockLogger.Object, maxSize: 10);
    }

    [Fact]
    public void Enqueue_AddsSingleSnapshot_Succeeds()
    {
        // Arrange
        var snapshot = CreateTestSnapshot();

        // Act
        _buffer.Enqueue(snapshot);

        // Assert
        var stats = _buffer.GetStats();
        Assert.Equal(1, stats.QueuedItems);
        Assert.Equal(1, stats.TotalEnqueued);
    }

    [Fact]
    public void TryDequeue_WithEnqueuedItem_ReturnsItem()
    {
        // Arrange
        var snapshot = CreateTestSnapshot();
        _buffer.Enqueue(snapshot);

        // Act
        var success = _buffer.TryDequeue(out var dequeued);

        // Assert
        Assert.True(success);
        Assert.NotNull(dequeued);
        Assert.Equal(snapshot.Data.Vehicle.SpeedKmh, dequeued.Data.Vehicle.SpeedKmh);
    }

    [Fact]
    public void TryDequeue_WithEmptyBuffer_ReturnsFalse()
    {
        // Act
        var success = _buffer.TryDequeue(out var dequeued);

        // Assert
        Assert.False(success);
        Assert.Null(dequeued);
    }

    [Fact]
    public void Enqueue_ExceedsMaxSize_DropsOldest()
    {
        // Arrange: Fill buffer to max size
        for (int i = 0; i < 10; i++)
        {
            _buffer.Enqueue(CreateTestSnapshot());
        }

        // Act: Add one more, should drop oldest
        _buffer.Enqueue(CreateTestSnapshot());

        // Assert
        var stats = _buffer.GetStats();
        Assert.Equal(10, stats.QueuedItems);
        Assert.Equal(11, stats.TotalEnqueued);
        Assert.Equal(1, stats.DroppedItems);
    }

    [Fact]
    public void GetStats_ReturnsAccurateStats()
    {
        // Arrange
        _buffer.Enqueue(CreateTestSnapshot());
        _buffer.TryDequeue(out _);
        _buffer.Enqueue(CreateTestSnapshot());

        // Act
        var stats = _buffer.GetStats();

        // Assert
        Assert.Equal(1, stats.QueuedItems);
        Assert.Equal(2, stats.TotalEnqueued);
        Assert.Equal(1, stats.TotalDequeued);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        _buffer.Enqueue(CreateTestSnapshot());
        _buffer.Enqueue(CreateTestSnapshot());

        // Act
        _buffer.Clear();

        // Assert
        var stats = _buffer.GetStats();
        Assert.Equal(0, stats.QueuedItems);
    }

    private static TelemetrySnapshot CreateTestSnapshot()
    {
        var data = new UnifiedTelemetryData
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Session = new SessionData
            {
                Id = Guid.NewGuid().ToString(),
                Game = "iRacing",
            },
            Vehicle = new VehicleData
            {
                SpeedKmh = 250f,
                Rpm = 8000,
                Gear = 4,
                FuelLiters = 50f,
                FuelConsumedThisLap = 1.5f,
                DistanceThisLapMeters = 3000f,
            },
        };

        return new TelemetrySnapshot(data);
    }
}
