using PitWall.Telemetry.Models;

namespace PitWall.Telemetry.WebSocket;

/// <summary>
/// Interface for WebSocket server that broadcasts telemetry to connected clients
/// </summary>
public interface IWebSocketServer
{
    /// <summary>
    /// Starts the WebSocket server
    /// </summary>
    Task StartAsync(int port, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the WebSocket server
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcasts telemetry data to all connected clients
    /// </summary>
    Task BroadcastTelemetryAsync(UnifiedTelemetryData telemetry);

    /// <summary>
    /// Gets the number of connected clients
    /// </summary>
    int ConnectedClientCount { get; }

    /// <summary>
    /// Fired when a client connects
    /// </summary>
    event EventHandler<EventArgs>? ClientConnected;

    /// <summary>
    /// Fired when a client disconnects
    /// </summary>
    event EventHandler<EventArgs>? ClientDisconnected;
}

/// <summary>
/// WebSocket server implementation (stub - will be implemented in Phase 1 Week 6-7)
/// </summary>
public sealed class WebSocketServer : IWebSocketServer
{
    private int _connectedClients;

    public int ConnectedClientCount => _connectedClients;

    public event EventHandler<EventArgs>? ClientConnected;
    public event EventHandler<EventArgs>? ClientDisconnected;

    public Task StartAsync(int port, CancellationToken cancellationToken = default)
    {
        // TODO: Implement WebSocket server startup
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement WebSocket server shutdown
        return Task.CompletedTask;
    }

    public Task BroadcastTelemetryAsync(UnifiedTelemetryData telemetry)
    {
        // TODO: Broadcast telemetry to all connected clients
        return Task.CompletedTask;
    }
}
