using PitWall.Telemetry.Models;

namespace PitWall.Telemetry.Connectors;

/// <summary>
/// Interface for racing sim connectors that parse raw UDP/API data
/// </summary>
public interface ISimConnector
{
    /// <summary>
    /// The name of the supported simulation
    /// </summary>
    string SimName { get; }

    /// <summary>
    /// Attempts to parse raw telemetry data from the sim
    /// </summary>
    /// <param name="rawData">Raw UDP packet or API response</param>
    /// <returns>Parsed telemetry data or null if parsing failed</returns>
    UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData);

    /// <summary>
    /// Validates if parsed data is consistent and correct
    /// </summary>
    bool IsValid(UnifiedTelemetryData data);

    /// <summary>
    /// Gets the expected UDP port for this sim (0 if not UDP-based)
    /// </summary>
    int? UdpPort { get; }
}

/// <summary>
/// Abstract base for sim connectors with common functionality
/// </summary>
public abstract class BaseSimConnector : ISimConnector
{
    protected readonly ISimConnector _normalizer;

    public abstract string SimName { get; }

    public abstract int? UdpPort { get; }

    protected BaseSimConnector()
    {
    }

    public abstract UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData);

    public abstract bool IsValid(UnifiedTelemetryData data);

    /// <summary>
    /// Helper to create session data from common fields
    /// </summary>
    protected SessionData CreateSession(string sessionId, string game, string? track = null)
    {
        return new SessionData
        {
            Id = sessionId,
            Game = game,
            Track = track ?? "Unknown",
            Type = "r",
        };
    }

    /// <summary>
    /// Helper to create vehicle data from common fields
    /// </summary>
    protected VehicleData CreateVehicle(
        float speedKmh,
        int rpm,
        int gear,
        float fuel)
    {
        return new VehicleData
        {
            SpeedKmh = speedKmh,
            Rpm = rpm,
            Gear = gear,
            FuelLiters = fuel,
            FuelConsumedThisLap = 0f,
            DistanceThisLapMeters = 0f,
        };
    }
}

/// <summary>
/// Factory for creating sim connectors
/// </summary>
public interface ISimConnectorFactory
{
    /// <summary>
    /// Gets a connector for the specified sim
    /// </summary>
    ISimConnector GetConnector(string simName);

    /// <summary>
    /// Registers a connector for a sim
    /// </summary>
    void RegisterConnector(ISimConnector connector);

    /// <summary>
    /// Gets all registered connectors
    /// </summary>
    IReadOnlyList<ISimConnector> GetAllConnectors();
}

/// <summary>
/// Implementation of sim connector factory
/// </summary>
public sealed class SimConnectorFactory : ISimConnectorFactory
{
    private readonly Dictionary<string, ISimConnector> _connectors;

    public SimConnectorFactory()
    {
        _connectors = new Dictionary<string, ISimConnector>();
    }

    public ISimConnector GetConnector(string simName)
    {
        if (!_connectors.TryGetValue(simName.ToLowerInvariant(), out var connector))
        {
            throw new KeyNotFoundException($"Connector not found for sim: {simName}");
        }

        return connector;
    }

    public void RegisterConnector(ISimConnector connector)
    {
        _connectors[connector.SimName.ToLowerInvariant()] = connector;
    }

    public IReadOnlyList<ISimConnector> GetAllConnectors()
    {
        return _connectors.Values.ToList();
    }
}
