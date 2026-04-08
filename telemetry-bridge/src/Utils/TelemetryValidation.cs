namespace PitWall.Telemetry.Utils;

/// <summary>
/// Telemetry validation utility functions
/// </summary>
public static class TelemetryValidation
{
    /// <summary>
    /// Validates vehicle speed is within acceptable racing range
    /// </summary>
    public static bool IsValidSpeed(float speedKmh)
    {
        return speedKmh >= 0 && speedKmh <= 400;
    }

    /// <summary>
    /// Validates RPM is within engine operating range
    /// </summary>
    public static bool IsValidRpm(int rpm)
    {
        return rpm >= 0 && rpm <= 20000;
    }

    /// <summary>
    /// Validates gear selection
    /// </summary>
    public static bool IsValidGear(int gear)
    {
        // -1 = Reverse, 0 = Neutral, 1+ = Forward gears
        return gear >= -1 && gear <= 10;
    }

    /// <summary>
    /// Validates fuel level
    /// </summary>
    public static bool IsValidFuel(float fuelLiters)
    {
        return fuelLiters >= 0 && fuelLiters <= 200;
    }

    /// <summary>
    /// Validates throttle input (0-1 normalized)
    /// </summary>
    public static bool IsValidThrottle(float throttle)
    {
        return throttle >= 0 && throttle <= 1;
    }

    /// <summary>
    /// Validates brake input (0-1 normalized)
    /// </summary>
    public static bool IsValidBrake(float brake)
    {
        return brake >= 0 && brake <= 1;
    }

    /// <summary>
    /// Validates steering angle (-1 to 1 normalized)
    /// </summary>
    public static bool IsValidSteering(float steeringAngle)
    {
        return steeringAngle >= -1 && steeringAngle <= 1;
    }

    /// <summary>
    /// Validates tire temperature in Celsius
    /// </summary>
    public static bool IsValidTireTemp(float tempCelsius)
    {
        return tempCelsius >= -40 && tempCelsius <= 150;
    }

    /// <summary>
    /// Validates tire wear percentage
    /// </summary>
    public static bool IsValidTireWear(float wearPercent)
    {
        return wearPercent >= 0 && wearPercent <= 100;
    }

    /// <summary>
    /// Validates lap time in seconds
    /// </summary>
    public static bool IsValidLapTime(float lapTimeSeconds)
    {
        return lapTimeSeconds > 0 && lapTimeSeconds < 3600; // Less than 1 hour
    }

    /// <summary>
    /// Validates delta time in seconds
    /// </summary>
    public static bool IsValidDelta(float deltaSeconds)
    {
        return deltaSeconds >= -60 && deltaSeconds <= 60; // +/- 1 minute
    }

    /// <summary>
    /// Validates air temperature in Celsius
    /// </summary>
    public static bool IsValidAirTemp(float tempCelsius)
    {
        return tempCelsius >= -40 && tempCelsius <= 60;
    }

    /// <summary>
    /// Validates track temperature in Celsius
    /// </summary>
    public static bool IsValidTrackTemp(float tempCelsius)
    {
        return tempCelsius >= 0 && tempCelsius <= 100;
    }

    /// <summary>
    /// Validates track grip coefficient
    /// </summary>
    public static bool IsValidTrackGrip(float gripCoeff)
    {
        return gripCoeff >= 0 && gripCoeff <= 1.5f; // Can exceed 1.0 in optimal conditions
    }

    /// <summary>
    /// Validates lap number
    /// </summary>
    public static bool IsValidLapNumber(int lapNumber)
    {
        return lapNumber >= 0 && lapNumber <= 2000;
    }

    /// <summary>
    /// Validates session UUID
    /// </summary>
    public static bool IsValidSessionId(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return false;

        return Guid.TryParse(sessionId, out _);
    }

    /// <summary>
    /// Validates game name
    /// </summary>
    public static bool IsValidGame(string? game)
    {
        return game?.ToLowerInvariant() switch
        {
            "iracing" => true,
            "acc" => true,
            "assetto corsa" => true,
            "f1-24" => true,
            "f1-25" => true,
            _ => false,
        };
    }

    /// <summary>
    /// Validates timestamp is reasonable (not too far in past/future)
    /// </summary>
    public static bool IsValidTimestamp(long timestampMs)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var fiveMinutesMs = 5 * 60 * 1000;

        // Accept if within 5 minutes of now
        return Math.Abs(now - timestampMs) <= fiveMinutesMs;
    }
}

/// <summary>
/// Telemetry sanitization utilities
/// </summary>
public static class TelemetrySanitization
{
    /// <summary>
    /// Clamps a value between min and max
    /// </summary>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;
        return value;
    }

    /// <summary>
    /// Rounds value to specified decimal places
    /// </summary>
    public static float RoundToDecimals(float value, int decimals = 2)
    {
        return MathF.Round(value, decimals);
    }

    /// <summary>
    /// Normalizes steering angle to -1 to 1 range
    /// </summary>
    public static float NormalizeSteering(float steeringRaw, float maxDegrees = 900f)
    {
        var normalized = steeringRaw / maxDegrees;
        return Clamp(normalized, -1f, 1f);
    }

    /// <summary>
    /// Converts RPM to percentage of max RPM
    /// </summary>
    public static float RpmToPercent(int rpm, int maxRpm = 13500)
    {
        return Clamp((float)rpm / maxRpm, 0f, 1f);
    }
}
