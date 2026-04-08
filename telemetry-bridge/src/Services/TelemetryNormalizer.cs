using Microsoft.Extensions.Logging;
using PitWall.Telemetry.Models;

namespace PitWall.Telemetry.Services;

/// <summary>
/// Interface for normalizing telemetry data from different racing simulations
/// </summary>
public interface ITelemetryNormalizer
{
    /// <summary>
    /// Validates telemetry data for completeness and correctness
    /// </summary>
    /// <param name="data">Telemetry data to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateTelemetry(UnifiedTelemetryData data);

    /// <summary>
    /// Gets validation errors for debugging
    /// </summary>
    /// <returns>List of validation error messages</returns>
    IReadOnlyList<string> GetValidationErrors();
}

/// <summary>
/// Service for normalizing telemetry data from all supported racing simulations
/// </summary>
public sealed class TelemetryNormalizer : ITelemetryNormalizer
{
    private readonly ILogger<TelemetryNormalizer> _logger;
    private readonly List<string> _validationErrors;

    public TelemetryNormalizer(ILogger<TelemetryNormalizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validationErrors = new List<string>();
    }

    /// <summary>
    /// Validates telemetry data using business rules specific to racing
    /// </summary>
    public bool ValidateTelemetry(UnifiedTelemetryData data)
    {
        _validationErrors.Clear();

        try
        {
            if (data == null)
            {
                _validationErrors.Add("Telemetry data is null");
                return false;
            }

            // Validate timestamp
            if (data.Timestamp <= 0)
            {
                _validationErrors.Add($"Invalid timestamp: {data.Timestamp}");
            }

            // Validate session
            if (data.Session == null)
            {
                _validationErrors.Add("Session data is required");
            }
            else
            {
                ValidateSession(data.Session);
            }

            // Validate vehicle
            if (data.Vehicle == null)
            {
                _validationErrors.Add("Vehicle data is required");
            }
            else
            {
                ValidateVehicle(data.Vehicle);
            }

            // Validate inputs
            if (data.Inputs != null)
            {
                ValidateInputs(data.Inputs);
            }

            // Validate tires
            if (data.Tires != null)
            {
                ValidateTires(data.Tires);
            }

            // Validate performance
            if (data.Performance != null)
            {
                ValidatePerformance(data.Performance);
            }

            return _validationErrors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating telemetry data");
            _validationErrors.Add($"Validation exception: {ex.Message}");
            return false;
        }
    }

    private void ValidateSession(SessionData session)
    {
        if (string.IsNullOrWhiteSpace(session.Id))
        {
            _validationErrors.Add("Session ID is required");
        }

        if (string.IsNullOrWhiteSpace(session.Game))
        {
            _validationErrors.Add("Game name is required");
        }
        else if (!IsValidGame(session.Game))
        {
            _validationErrors.Add($"Unknown game: {session.Game}");
        }
    }

    private void ValidateVehicle(VehicleData vehicle)
    {
        if (vehicle.SpeedKmh < 0 || vehicle.SpeedKmh > 400)
        {
            _validationErrors.Add($"Speed out of range: {vehicle.SpeedKmh}");
        }

        if (vehicle.Rpm < 0 || vehicle.Rpm > 20000)
        {
            _validationErrors.Add($"RPM out of range: {vehicle.Rpm}");
        }

        if (vehicle.Gear < -1 || vehicle.Gear > 10)
        {
            _validationErrors.Add($"Gear out of range: {vehicle.Gear}");
        }

        if (vehicle.FuelLiters < 0 || vehicle.FuelLiters > 200)
        {
            _validationErrors.Add($"Fuel level out of range: {vehicle.FuelLiters}");
        }
    }

    private void ValidateInputs(InputData inputs)
    {
        if (inputs.Throttle < 0 || inputs.Throttle > 1)
        {
            _validationErrors.Add($"Throttle out of range: {inputs.Throttle}");
        }

        if (inputs.Brake < 0 || inputs.Brake > 1)
        {
            _validationErrors.Add($"Brake out of range: {inputs.Brake}");
        }

        if (inputs.SteeringAngle < -1 || inputs.SteeringAngle > 1)
        {
            _validationErrors.Add($"Steering angle out of range: {inputs.SteeringAngle}");
        }
    }

    private void ValidateTires(TireData tires)
    {
        ValidateTireInfo("Front Left", tires.FrontLeft);
        ValidateTireInfo("Front Right", tires.FrontRight);
        ValidateTireInfo("Rear Left", tires.RearLeft);
        ValidateTireInfo("Rear Right", tires.RearRight);
    }

    private void ValidateTireInfo(string position, TireInfo? tire)
    {
        if (tire == null)
            return;

        if (tire.WearPercent < 0 || tire.WearPercent > 100)
        {
            _validationErrors.Add($"{position} tire wear out of range: {tire.WearPercent}");
        }

        if (tire.TemperatureCelsius != null)
        {
            foreach (var temp in tire.TemperatureCelsius)
            {
                if (temp < -40 || temp > 150)
                {
                    _validationErrors.Add($"{position} tire temperature out of range: {temp}");
                }
            }
        }
    }

    private void ValidatePerformance(PerformanceData performance)
    {
        if (performance.LapNumber < 0)
        {
            _validationErrors.Add($"Invalid lap number: {performance.LapNumber}");
        }

        if (performance.CurrentLapTime.HasValue && performance.CurrentLapTime < 0)
        {
            _validationErrors.Add($"Invalid lap time: {performance.CurrentLapTime}");
        }
    }

    public IReadOnlyList<string> GetValidationErrors()
    {
        return _validationErrors.AsReadOnly();
    }

    private static bool IsValidGame(string game)
    {
        return game.ToLowerInvariant() switch
        {
            "iracing" => true,
            "acc" => true,
            "assetto corsa" => true,
            "assetto corsa competizione" => true,
            "f1-24" => true,
            "f1-25" => true,
            _ => false,
        };
    }
}
