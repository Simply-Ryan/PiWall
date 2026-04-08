using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PitWall.Telemetry.WebSocket;

namespace PitWall.Telemetry.Services;

/// <summary>
/// Main background service that coordinates telemetry collection and broadcasting
/// </summary>
public sealed class TelemetryService : BackgroundService
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly IWebSocketServer _webSocketServer;

    public TelemetryService(
        ILogger<TelemetryService> logger,
        IWebSocketServer webSocketServer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketServer = webSocketServer ?? throw new ArgumentNullException(nameof(webSocketServer));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelemetryService starting");

        try
        {
            // TODO: Week 3-6: Implement individual connectors for each sim
            // - iRacing UDP listener
            // - ACC API connector
            // - Assetto Corsa UDP listener
            // - F1 24/25 API connector

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("TelemetryService cancellation requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TelemetryService");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TelemetryService stopping");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Hosted service that manages WebSocket server lifecycle
/// </summary>
public sealed class WebSocketHostedService : BackgroundService
{
    private readonly ILogger<WebSocketHostedService> _logger;
    private readonly IWebSocketServer _webSocketServer;
    private readonly int _port = 43200; // Default WebSocket port

    public WebSocketHostedService(
        ILogger<WebSocketHostedService> logger,
        IWebSocketServer webSocketServer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketServer = webSocketServer ?? throw new ArgumentNullException(nameof(webSocketServer));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting WebSocket server on port {Port}", _port);

        try
        {
            await _webSocketServer.StartAsync(_port, stoppingToken);
            _logger.LogInformation("WebSocket server started successfully");

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("WebSocket service cancellation requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting WebSocket server");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping WebSocket server");
        await _webSocketServer.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
