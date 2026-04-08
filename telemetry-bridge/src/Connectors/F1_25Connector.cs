using System;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Connectors
{
    /// <summary>
    /// F1 25 telemetry UDP connector implementation.
    /// Parses binary telemetry packets from F1 25 on UDP port 20777.
    /// 
    /// F1 25 is expected to use a similar telemetry format to F1 24,
    /// with potential enhancements based on Codemasters updates.
    /// 
    /// Note: As of April 2026, F1 25 API details are not yet publicly available.
    /// This connector is structured similarly to F1Connector and will be updated
    /// when the official F1 25 telemetry format is released.
    /// </summary>
    public class F1_25Connector : BaseSimConnector
    {
        // F1 25 UDP constants (expected to mirror F1 24)
        private const int F1_25_PACKET_HEADER_SIZE = 24;
        private const int F1_25_MAX_PACKET_SIZE = 2048;
        
        // UDP port for F1 telemetry (same as F1 24)
        public override int DefaultPort => 20777;
        public override string SimulatorName => "F1 25";

        /// <summary>
        /// Parses F1 25 binary telemetry packet into UnifiedTelemetryData.
        /// 
        /// Structure mirrors F1 24 until official specs are released.
        /// Expected format changes from F1 24:
        /// - Potentially updated motion data fields
        /// - New car features (hybrid power, aero adjustments)
        /// - Enhanced session data for new game modes
        /// - Expanded driver stats tracking
        /// 
        /// For now, this delegates to F1Connector with version detection.
        /// </summary>
        /// <param name="rawData">Raw UDP packet bytes from F1 25</param>
        /// <returns>Parsed UnifiedTelemetryData or null if parsing fails</returns>
        public override UnifiedTelemetryData? Parse(byte[] rawData)
        {
            try
            {
                // Validate packet size
                if (rawData == null || rawData.Length < F1_25_PACKET_HEADER_SIZE)
                {
                    RecordParsingError($"Invalid F1 25 packet size: {rawData?.Length ?? 0}");
                    return null;
                }

                // For now, attempt to parse using F1 24 format
                // This will gracefully handle F1 25 packets if they follow similar structure
                var f1Connector = new F1Connector();
                var result = f1Connector.Parse(rawData);

                if (result != null)
                {
                    // Override simulator name to distinguish from F1 24
                    result.SessionData.SimulatorName = SimulatorName;
                }

                return result;
            }
            catch (Exception ex)
            {
                RecordParsingError($"F1 25 parsing exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Records parsing errors for debugging.
        /// </summary>
        private void RecordParsingError(string error)
        {
            System.Console.WriteLine($"[F1 25 Parser] {error}");
        }
    }
}
