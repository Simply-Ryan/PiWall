using System.ComponentModel.DataAnnotations;

namespace PitWall.Telemetry.Models;

/// <summary>
/// Domain-specific errors for telemetry operations
/// </summary>
public abstract class TelemetryDomainError : Exception
{
    /// <summary>
    /// Error code for categorization and logging
    /// </summary>
    public string Code { get; protected set; }

    protected TelemetryDomainError(string message, string code)
        : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// Thrown when telemetry data fails validation
/// </summary>
public class TelemetryValidationException : TelemetryDomainError
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public TelemetryValidationException(
        string message,
        IReadOnlyList<string> validationErrors)
        : base(message, "VALIDATION_FAILED")
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Thrown when a telemetry parsing operation fails
/// </summary>
public class TelemetryParsingException : TelemetryDomainError
{
    public string? RawData { get; }
    public string? SourceGame { get; }

    public TelemetryParsingException(
        string message,
        string? sourceGame = null,
        string? rawData = null)
        : base(message, "PARSING_FAILED")
    {
        SourceGame = sourceGame;
        RawData = rawData;
    }
}

/// <summary>
/// Represents a racing session identifier and metadata
/// </summary>
public sealed class SessionIdentifier
{
    private readonly string _value;

    [Range(36, 36)]
    public string Value => _value;

    public string Game { get; }

    private SessionIdentifier(string value, string game)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 36)
            throw new ArgumentException("Session ID must be a valid UUID", nameof(value));

        _value = value;
        Game = game;
    }

    /// <summary>
    /// Creates a new session identifier
    /// </summary>
    public static SessionIdentifier Create(string game)
    {
        return new SessionIdentifier(Guid.NewGuid().ToString(), game);
    }

    /// <summary>
    /// Parses an existing session identifier
    /// </summary>
    public static SessionIdentifier Parse(string id, string game)
    {
        return new SessionIdentifier(id, game);
    }

    public override string ToString() => _value;

    public override bool Equals(object? obj) =>
        obj is SessionIdentifier other && other.Value == Value;

    public override int GetHashCode() => Value.GetHashCode();
}

/// <summary>
/// Telemetry data point with timestamp
/// </summary>
public sealed class TelemetrySnapshot
{
    public long Timestamp { get; set; }

    public UnifiedTelemetryData Data { get; set; } = null!;

    /// <summary>
    /// Milliseconds since last update
    /// </summary>
    public int DeltaTimeMs { get; set; }

    public TelemetrySnapshot()
    {
    }

    public TelemetrySnapshot(UnifiedTelemetryData data)
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Data = data;
    }
}

/// <summary>
/// Aggregated telemetry statistics for a session
/// </summary>
public sealed class TelemetryStatistics
{
    public SessionIdentifier Session { get; set; } = null!;

    [Range(0, 1000)]
    public int UpdateCount { get; set; }

    [Range(0, 60000)]
    public double AverageLatencyMs { get; set; }

    [Range(0, 60000)]
    public double MaxLatencyMs { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Range(0, 100)]
    public float DataCompleteness { get; set; }

    public Dictionary<string, int> ErrorsByType { get; } = new();
}

/// <summary>
/// Configuration for telemetry recording
/// </summary>
public sealed class TelemetryRecordingConfig
{
    [Range(1, 1000)]
    public int BufferSizeKb { get; set; } = 512;

    [Range(1, 100)]
    public int MaxStorageGb { get; set; } = 50;

    public bool CompressData { get; set; } = true;

    public List<string> EnabledFields { get; } = new()
    {
        "timestamp",
        "vehicle",
        "tires",
        "fuel",
        "performance",
        "inputs"
    };
}

/// <summary>
/// Health status of telemetry connection
/// </summary>
public sealed class TelemetryConnectionHealth
{
    public bool IsConnected { get; set; }

    public string? ConnectedGame { get; set; }

    [Range(0, 60000)]
    public double AverageLatencyMs { get; set; }

    [Range(0, 100)]
    public float PacketLossPercent { get; set; }

    public DateTime LastUpdate { get; set; }

    public DateTime? LastError { get; set; }

    public string? LastErrorMessage { get; set; }

    [Range(0, 100)]
    public float HealthPercent => CalculateHealth();

    private float CalculateHealth()
    {
        if (!IsConnected)
            return 0;

        float score = 100f;

        // Deduct for latency
        if (AverageLatencyMs > 20)
            score -= Math.Min(30, (float)AverageLatencyMs / 2);

        // Deduct for packet loss
        score -= PacketLossPercent * 0.5f;

        return Math.Max(0, score);
    }
}
