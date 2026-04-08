using PitWall.Telemetry.Models;

namespace PitWall.Telemetry.Connectors;

/// <summary>
/// Stub implementation for iRacing connector
/// Full implementation in Phase 1 Week 3-4
/// </summary>
public sealed class IracingConnector : BaseSimConnector
{
    public override string SimName => "iRacing";

    public override int? UdpPort => 11111;

    public override UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData)
    {
        // TODO: Implement iRacing UDP packet parsing
        // iRacing sends telemetry via UDP on port 11111
        // Parse binary format into UnifiedTelemetryData
        return null;
    }

    public override bool IsValid(UnifiedTelemetryData data)
    {
        // TODO: Implement iRacing-specific validation
        return data?.Session?.Game == "iRacing";
    }
}

/// <summary>
/// Stub implementation for Assetto Corsa Competizione connector
/// Full implementation in Phase 1 Week 4-5
/// </summary>
public sealed class AccConnector : BaseSimConnector
{
    public override string SimName => "ACC";

    public override int? UdpPort => 9996;

    public override UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData)
    {
        // TODO: Implement ACC UDP packet parsing
        // ACC sends telemetry via UDP on port 9996
        // Parse binary format into UnifiedTelemetryData
        return null;
    }

    public override bool IsValid(UnifiedTelemetryData data)
    {
        // TODO: Implement ACC-specific validation
        return data?.Session?.Game == "ACC";
    }
}

/// <summary>
/// Stub implementation for Assetto Corsa connector
/// Full implementation in Phase 1 Week 4-5
/// </summary>
public sealed class AssettoGorConnector : BaseSimConnector
{
    public override string SimName => "Assetto Corsa";

    public override int? UdpPort => 10000;

    public override UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData)
    {
        // TODO: Implement Assetto Corsa UDP packet parsing
        // AC sends telemetry via UDP on port 10000
        // Parse binary format into UnifiedTelemetryData
        return null;
    }

    public override bool IsValid(UnifiedTelemetryData data)
    {
        // TODO: Implement AC-specific validation
        return data?.Session?.Game == "AC";
    }
}

/// <summary>
/// Stub implementation for F1 24/25 connector
/// Full implementation in Phase 1 Week 5-6
/// </summary>
public sealed class F124Connector : BaseSimConnector
{
    public override string SimName => "F1-24";

    public override int? UdpPort => 20777;

    public override UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData)
    {
        // TODO: Implement F1 24 UDP packet parsing
        // F1 24 sends telemetry via UDP on port 20777
        // Parse binary format into UnifiedTelemetryData
        return null;
    }

    public override bool IsValid(UnifiedTelemetryData data)
    {
        // TODO: Implement F1-specific validation
        return data?.Session?.Game == "F1-24";
    }
}

/// <summary>
/// Stub implementation for F1 25 connector
/// Full implementation in Phase 1 Week 5-6 (if API available)
/// </summary>
public sealed class F125Connector : BaseSimConnector
{
    public override string SimName => "F1-25";

    public override int? UdpPort => 20777;

    public override UnifiedTelemetryData? Parse(ReadOnlySpan<byte> rawData)
    {
        // TODO: Implement F1 25 UDP packet parsing
        // F1 25 likely uses similar format to F1 24
        // Will confirm upon release
        return null;
    }

    public override bool IsValid(UnifiedTelemetryData data)
    {
        // TODO: Implement F1 25-specific validation
        return data?.Session?.Game == "F1-25";
    }
}
