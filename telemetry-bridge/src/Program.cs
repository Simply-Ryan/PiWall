using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PitWall.Telemetry.Services;
using PitWall.Telemetry.WebSocket;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging
            .ClearProviders()
            .AddConsole();

        if (context.HostingEnvironment.IsDevelopment())
        {
            logging.SetMinimumLevel(LogLevel.Debug);
        }
    })
    .ConfigureServices((context, services) =>
    {
        // Register services
        services.AddSingleton<ITelemetryNormalizer, TelemetryNormalizer>();
        services.AddSingleton<IWebSocketServer, WebSocketServer>();
        
        // Register hosted services
        services.AddHostedService<TelemetryService>();
        services.AddHostedService<WebSocketHostedService>();
    });

var host = builder.Build();

try
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting PitWall Telemetry Bridge v0.1.0");
    logger.LogInformation("Environment: {Environment}", 
        host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex}");
    Environment.Exit(1);
}
