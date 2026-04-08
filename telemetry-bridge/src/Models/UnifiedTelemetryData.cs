using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PitWall.Telemetry.Models;

/// <summary>
/// Unified telemetry data model used across all racing simulations.
/// This schema normalizes data from iRacing, ACC, Assetto Corsa, and F1 24/25.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class UnifiedTelemetryData
{
    /// <summary>
    /// Unix timestamp in milliseconds when telemetry was captured
    /// </summary>
    [JsonProperty("timestamp")]
    [Required]
    public long Timestamp { get; set; }

    /// <summary>
    /// Session information
    /// </summary>
    [JsonProperty("session")]
    [Required]
    public SessionData Session { get; set; } = null!;

    /// <summary>
    /// Vehicle data
    /// </summary>
    [JsonProperty("vehicle")]
    [Required]
    public VehicleData Vehicle { get; set; } = null!;

    /// <summary>
    /// Driver input data
    /// </summary>
    [JsonProperty("inputs")]
    public InputData? Inputs { get; set; }

    /// <summary>
    /// Tire data with temperatures and wear
    /// </summary>
    [JsonProperty("tires")]
    public TireData? Tires { get; set; }

    /// <summary>
    /// Performance metrics
    /// </summary>
    [JsonProperty("performance")]
    public PerformanceData? Performance { get; set; }

    /// <summary>
    /// Environmental conditions
    /// </summary>
    [JsonProperty("environment")]
    public EnvironmentData? Environment { get; set; }
}

/// <summary>
/// Session information
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class SessionData
{
    [JsonProperty("id")]
    [Required]
    [StringLength(36)]
    public string Id { get; set; } = null!;

    [JsonProperty("name")]
    [StringLength(256)]
    public string? Name { get; set; }

    /// <summary>
    /// Session type: q (qualifying), r (race), p (practice), w (warmup)
    /// </summary>
    [JsonProperty("type")]
    [StringLength(1)]
    public string? Type { get; set; }

    [JsonProperty("track")]
    [StringLength(256)]
    public string? Track { get; set; }

    [JsonProperty("game")]
    [Required]
    [StringLength(50)]
    public string Game { get; set; } = null!;

    [JsonProperty("duration_seconds")]
    public int? DurationSeconds { get; set; }
}

/// <summary>
/// Vehicle dynamics and status
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class VehicleData
{
    [JsonProperty("speed_kmh")]
    [Range(0, 400)]
    public float SpeedKmh { get; set; }

    [JsonProperty("gear")]
    [Range(-1, 10)]
    public int Gear { get; set; }

    [JsonProperty("rpm")]
    [Range(0, 20000)]
    public int Rpm { get; set; }

    [JsonProperty("fuel_liters")]
    [Range(0, 200)]
    public float FuelLiters { get; set; }

    [JsonProperty("fuel_consumed_this_lap")]
    [Range(0, 100)]
    public float FuelConsumedThisLap { get; set; }

    [JsonProperty("distance_this_lap_m")]
    public float DistanceThisLapMeters { get; set; }
}

/// <summary>
/// Driver input data
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class InputData
{
    [JsonProperty("throttle")]
    [Range(0, 1)]
    public float Throttle { get; set; }

    [JsonProperty("brake")]
    [Range(0, 1)]
    public float Brake { get; set; }

    [JsonProperty("steering_angle")]
    [Range(-1, 1)]
    public float SteeringAngle { get; set; }

    [JsonProperty("clutch")]
    [Range(0, 1)]
    public float? Clutch { get; set; }
}

/// <summary>
/// Tire information
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class TireData
{
    [JsonProperty("fl")]
    public TireInfo? FrontLeft { get; set; }

    [JsonProperty("fr")]
    public TireInfo? FrontRight { get; set; }

    [JsonProperty("rl")]
    public TireInfo? RearLeft { get; set; }

    [JsonProperty("rr")]
    public TireInfo? RearRight { get; set; }
}

/// <summary>
/// Single tire information
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class TireInfo
{
    /// <summary>
    /// Temperature in Celsius: [inner, middle, outer]
    /// </summary>
    [JsonProperty("temp_c")]
    public float[]? TemperatureCelsius { get; set; }

    /// <summary>
    /// Tire wear percentage (0-100)
    /// </summary>
    [JsonProperty("wear_pct")]
    [Range(0, 100)]
    public float WearPercent { get; set; }
}

/// <summary>
/// Performance metrics
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class PerformanceData
{
    [JsonProperty("delta_to_best")]
    public float? DeltaToBestLap { get; set; }

    [JsonProperty("delta_to_session_avg")]
    public float? DeltaToSessionAverage { get; set; }

    [JsonProperty("num_laps")]
    public int LapNumber { get; set; }

    [JsonProperty("current_lap_time")]
    public float? CurrentLapTime { get; set; }
}

/// <summary>
/// Environmental conditions
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class EnvironmentData
{
    [JsonProperty("air_temp_c")]
    [Range(-40, 60)]
    public float? AirTemperatureCelsius { get; set; }

    [JsonProperty("track_temp_c")]
    [Range(0, 100)]
    public float? TrackTemperatureCelsius { get; set; }

    [JsonProperty("weather")]
    [StringLength(50)]
    public string? Weather { get; set; }

    [JsonProperty("track_grip")]
    [Range(0, 1)]
    public float? TrackGrip { get; set; }
}
