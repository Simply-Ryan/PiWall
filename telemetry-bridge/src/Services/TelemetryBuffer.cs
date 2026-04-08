using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PitWall.Telemetry.Models;

namespace PitWall.Telemetry.Services;

/// <summary>
/// Interface for telemetry data buffering and processing
/// </summary>
public interface ITelemetryBuffer
{
    /// <summary>
    /// Enqueues a telemetry snapshot for processing
    /// </summary>
    void Enqueue(TelemetrySnapshot snapshot);

    /// <summary>
    /// Dequeues the next telemetry snapshot, if available
    /// </summary>
    bool TryDequeue(out TelemetrySnapshot? snapshot);

    /// <summary>
    /// Gets current buffer statistics
    /// </summary>
    TelemetryBufferStats GetStats();

    /// <summary>
    /// Clears all pending snapshots
    /// </summary>
    void Clear();
}

/// <summary>
/// Statistics about buffer state
/// </summary>
public sealed class TelemetryBufferStats
{
    public int QueuedItems { get; set; }

    public int MaxQueueSize { get; set; }

    public long TotalEnqueued { get; set; }

    public long TotalDequeued { get; set; }

    public int DroppedItems { get; set; }

    public double AgeOfOldestItemMs { get; set; }
}

/// <summary>
/// Thread-safe buffer for telemetry data processing
/// Implements a bounded queue with overflow handling
/// </summary>
public sealed class TelemetryBuffer : ITelemetryBuffer
{
    private readonly ConcurrentQueue<TelemetrySnapshot> _queue;
    private readonly int _maxSize;
    private readonly ILogger<TelemetryBuffer> _logger;

    private long _totalEnqueued;
    private long _totalDequeued;
    private int _droppedItems;

    public TelemetryBuffer(
        ILogger<TelemetryBuffer> logger,
        int maxSize = 1000)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxSize = maxSize;
        _queue = new ConcurrentQueue<TelemetrySnapshot>();
    }

    public void Enqueue(TelemetrySnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        if (_queue.Count >= _maxSize)
        {
            // Drop oldest item to maintain max size
            if (_queue.TryDequeue(out _))
            {
                Interlocked.Increment(ref _droppedItems);
                _logger.LogWarning(
                    "Telemetry buffer overflow: dropped item. Queue size: {QueueSize}",
                    _queue.Count);
            }
        }

        _queue.Enqueue(snapshot);
        Interlocked.Increment(ref _totalEnqueued);
    }

    public bool TryDequeue(out TelemetrySnapshot? snapshot)
    {
        var success = _queue.TryDequeue(out snapshot);
        if (success)
        {
            Interlocked.Increment(ref _totalDequeued);
        }

        return success;
    }

    public TelemetryBufferStats GetStats()
    {
        var oldestAge = 0.0;
        if (_queue.TryPeek(out var oldest) && oldest != null)
        {
            oldestAge = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - oldest.Timestamp;
        }

        return new TelemetryBufferStats
        {
            QueuedItems = _queue.Count,
            MaxQueueSize = _maxSize,
            TotalEnqueued = Interlocked.Read(ref _totalEnqueued),
            TotalDequeued = Interlocked.Read(ref _totalDequeued),
            DroppedItems = Interlocked.Read(ref _droppedItems),
            AgeOfOldestItemMs = oldestAge,
        };
    }

    public void Clear()
    {
        while (_queue.TryDequeue(out _)) { }
        _logger.LogInformation("Telemetry buffer cleared");
    }
}
