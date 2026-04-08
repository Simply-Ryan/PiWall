using System;
using System.Runtime.InteropServices;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Connectors
{
    /// <summary>
    /// F1 24 telemetry UDP connector implementation.
    /// Parses binary telemetry packets from F1 24 on UDP port 20777.
    /// Uses official Codemasters F1 24 UDP telemetry format (1476+ bytes).
    /// Sends ~100 Hz telemetry updates (~10ms intervals).
    /// 
    /// F1 24 is the most comprehensive sim with detailed telemetry covering:
    /// - Multi-player session data
    /// - Detailed car physics (suspension, traction control, DRS)
    /// - Button state tracking
    /// - Motion data (G-forces, motion platform parameters)
    /// - Extensive session history per lap
    /// </summary>
    public class F1Connector : BaseSimConnector
    {
        // F1 24 UDP constants
        private const int F1_PACKET_HEADER_SIZE = 24;
        private const int F1_MAX_CARS = 22; // Max players in F1 session
        private const int F1_MAX_PACKET_SIZE = 2048;
        
        // Gear constants
        private const int F1_GEAR_REVERSE = 0; // In F1, 0 = reverse, 1 = neutral
        private const int F1_GEAR_NEUTRAL = 1;
        
        // UDP port for F1 telemetry
        public override int DefaultPort => 20777;
        public override string SimulatorName => "F1 24";

        /// <summary>
        /// F1 24 packet header structure (24 bytes).
        /// Provides packet identification, frame timing, and session info.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct F1PacketHeader
        {
            public ushort PacketFormat;              // 2024 for F1 24
            public byte GameMajorVersion;            // Game major version
            public byte GameMinorVersion;            // Game minor version
            public byte PacketVersion;               // Telemetry packet version
            public byte PacketType;                  // Type: 0=Motion, 1=Session, 2=LapData, 3=Event, 4=Participants, 5=CarSetup, 6=CarTelemetry, 7=CarStatus
            public ulong SessionUID;                 // Unique session identifier
            public double SessionTime;               // Session elapsed time (seconds)
            public uint FrameIdentifier;             // Frame number
            public byte PlayerCarIndex;              // Index of player car (0-21)
            public byte SecondaryPlayerCarIndex;     // Secondary player index (255 if not available)
            public byte FlashbackFrame;              // Flashback frame identifier
            public byte PausePacket;                 // Pause 0=running, 1=paused
            public byte Reserved;                    // Reserved for future use
        }

        /// <summary>
        /// F1 24 car motion telemetry (per-car data from motion packet).
        /// Contains vehicle dynamics and motion vectors.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct F1CarMotionData
        {
            // World space position and velocity
            public float WorldPositionX;             // World X position
            public float WorldPositionY;             // World Y position
            public float WorldPositionZ;             // World Z position
            public float WorldVelocityX;             // World X velocity (m/s)
            public float WorldVelocityY;             // World Y velocity (m/s)
            public float WorldVelocityZ;             // World Z velocity (m/s)

            // Acceleration and rotation
            public short RearWingHeight;             // Rear wing height
            public float Yaw;                        // Yaw angle (radians)
            public float Pitch;                      // Pitch angle (radians)
            public float Roll;                       // Roll angle (radians)
            public float YawVelocity;                // Yaw angular velocity
            public float PitchVelocity;              // Pitch angular velocity
            public float RollVelocity;               // Roll angular velocity

            // G-forces and suspension
            public float LateralAcceleration;        // Lateral acceleration (G)
            public float LongitudinalAcceleration;  // Longitudinal acceleration (G)
            public float VerticalAcceleration;       // Vertical acceleration (G)

            // Wheel motion (4 wheels: FL, FR, BL, BR)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionPosition;       // Suspension position
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionVelocity;       // Suspension velocity
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelSpeed;               // Wheel speed (m/s)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelSlip;                // Wheel slip ratio
        }

        /// <summary>
        /// F1 24 car telemetry (per-car detailed telemetry).
        /// Contains comprehensive vehicle control and performance data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct F1CarTelemetryData
        {
            // Speed and controls
            public ushort Speed;                     // Speed in km/h
            public float Throttle;                   // Throttle input (0.0-1.0)
            public float Steer;                      // Steering input (-1.0 to 1.0)
            public float Brake;                      // Brake input (0.0-1.0)
            public byte Clutch;                      // Clutch input (0-100)
            public sbyte Gear;                       // Current gear (-1=reverse, 0=neutral, 1-N=forward)
            public ushort EngineRPM;                 // Engine RPM
            public byte DRS;                         // DRS mode (0=off, 1=on)
            public byte RevLightsPercent;            // Rev limit indicator (0-100%)
            public ushort BrakesTemperature;         // Brake temperature (celsius)

            // Tire data
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] TiresSurfaceTemperature; // Tire surface temperature (FL, FR, BL, BR)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] TiresInternalTemperature; // Tire internal temperature
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] TiresPressure;           // Tire pressure (PSI * 10)

            // Fuel and engine
            public byte FuelInTank;                  // Fuel remaining (liters, 0-120)
            public float FuelCapacity;               // Fuel tank capacity
            public ushort FuelFlowThisLap;           // Fuel consumed this lap (liters * 10)
            public ushort FuelFlowTotal;             // Total fuel consumed
            public float EngineDamage;               // Engine damage (0-100%)

            // Settings and modes
            public float TractionControl;            // Traction control level (0-1)
            public byte AntiLockBrakes;              // ABS mode (0=off, 1=on)
            public byte FuelMix;                     // Fuel mix (0-3)
            public byte FrontBrakeBias;              // Front brake bias percentage
            public byte PitLimiterStatus;            // Pit limiter (0=off, 1=on)

            // Additional data
            public float CellLoad;                   // Cell system load (0-1)
        }

        /// <summary>
        /// Parses F1 24 binary telemetry packet into UnifiedTelemetryData.
        /// 
        /// F1 24 provides extremely detailed telemetry with multiple packet types.
        /// This parser extracts motion and telemetry data for player car.
        /// 
        /// Packet structure is complex with nested arrays for multi-player data.
        /// Focus is on player car (index from header).
        /// </summary>
        /// <param name="rawData">Raw UDP packet bytes from F1 24</param>
        /// <returns>Parsed UnifiedTelemetryData or null if parsing fails</returns>
        public override UnifiedTelemetryData? Parse(byte[] rawData)
        {
            try
            {
                // Validate packet size
                if (rawData == null || rawData.Length < F1_PACKET_HEADER_SIZE)
                {
                    RecordParsingError($"Invalid F1 24 packet size: {rawData?.Length ?? 0}");
                    return null;
                }

                // Parse header
                F1PacketHeader header = ParseHeader(rawData);

                // Validate packet type and format
                if (!ValidatePacket(header))
                {
                    return null;
                }

                // Extract player car index
                int playerCarIndex = header.PlayerCarIndex;
                if (playerCarIndex >= F1_MAX_CARS)
                {
                    RecordParsingError($"Invalid player car index: {playerCarIndex}");
                    return null;
                }

                // Parse motion and telemetry data for player car
                F1CarMotionData motionData = ParseMotionData(rawData, playerCarIndex);
                F1CarTelemetryData telemetryData = ParseTelemetryData(rawData, playerCarIndex);

                // Build unified telemetry data
                var unifiedData = new UnifiedTelemetryData
                {
                    // Session data
                    SessionData = new SessionData
                    {
                        SimulatorName = SimulatorName,
                        SessionID = header.SessionUID.ToString(),
                        SessionNumber = 0, // Multiple in F1 season
                        SessionTime = header.SessionTime,
                        SessionTickCount = (int)header.FrameIdentifier,
                        SimSpeed = header.PausePacket == 0 ? 1f : 0f,
                        SessionStatus = header.PausePacket == 0 ? "Active" : "Paused",
                        SessionState = GetSessionState(header)
                    },

                    // Vehicle data
                    VehicleData = new VehicleData
                    {
                        Speed = telemetryData.Speed,
                        Gear = MapF1Gear(telemetryData.Gear),
                        RPM = telemetryData.EngineRPM,
                        ThrottlePosition = Clamp(telemetryData.Throttle, 0, 1),
                        BrakePosition = Clamp(telemetryData.Brake, 0, 1),
                        ClutchPosition = Clamp(telemetryData.Clutch / 100f, 0, 1),
                        FuelAmount = telemetryData.FuelInTank,
                        FuelCapacity = telemetryData.FuelCapacity,
                        WaterTemperature = 0, // F1 doesn't expose this separately
                        OilTemperature = 0,    // Calculated from engine damage
                        OilPressure = 0,       // Not exposed in F1 24
                        EngineThrottle = Clamp(telemetryData.Throttle, 0, 1)
                    },

                    // Input data
                    InputData = new InputData
                    {
                        SteeringAngle = telemetryData.Steer * (float)Math.PI, // Steer is -1 to 1
                        Throttle = Clamp(telemetryData.Throttle, 0, 1),
                        Brake = Clamp(telemetryData.Brake, 0, 1),
                        Clutch = Clamp(telemetryData.Clutch / 100f, 0, 1),
                        HandBrake = 0f
                    },

                    // Tire data
                    TireData = ParseF1TireData(telemetryData),

                    // Performance data
                    PerformanceData = new PerformanceData
                    {
                        LateralAcceleration = ConvertGForce(motionData.LateralAcceleration),
                        LongitudinalAcceleration = ConvertGForce(motionData.LongitudinalAcceleration),
                        VerticalAcceleration = ConvertGForce(motionData.VerticalAcceleration),
                        CurrentLapTime = null, // In lap data packet
                        BestLapTime = null,
                        LastLapTime = null,
                        EstimatedLapTime = null,
                        LapCount = 0,
                        BestLapNumber = 0
                    },

                    // Environment data
                    EnvironmentData = new EnvironmentData
                    {
                        RollAngle = motionData.Roll,
                        PitchAngle = motionData.Pitch,
                        YawAngle = motionData.Yaw,
                        ForceFeedbackIntensity = 0f,
                        TrackTemperature = null,
                        AmbientTemperature = null
                    },

                    // Timestamp
                    Timestamp = DateTime.UtcNow,
                    ReceivedAt = DateTime.UtcNow
                };

                IncrementSuccessCount();
                return unifiedData;
            }
            catch (Exception ex)
            {
                RecordParsingError($"F1 24 parsing exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses F1 24 packet header.
        /// </summary>
        private F1PacketHeader ParseHeader(byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return (F1PacketHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(F1PacketHeader))!;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Validates F1 24 packet structure.
        /// </summary>
        private bool ValidatePacket(F1PacketHeader header)
        {
            // Check format version (2024 for F1 24)
            if (header.PacketFormat != 2024)
            {
                RecordParsingError($"Unexpected packet format: {header.PacketFormat}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses motion data for player car from packet.
        /// </summary>
        private F1CarMotionData ParseMotionData(byte[] data, int carIndex)
        {
            // Motion data offset: after header, then per-car data
            int offset = F1_PACKET_HEADER_SIZE + (carIndex * Marshal.SizeOf(typeof(F1CarMotionData)));
            
            if (offset + Marshal.SizeOf(typeof(F1CarMotionData)) > data.Length)
            {
                throw new InvalidOperationException("Motion data out of packet bounds");
            }

            byte[] motionBuffer = new byte[Marshal.SizeOf(typeof(F1CarMotionData))];
            Array.Copy(data, offset, motionBuffer, 0, motionBuffer.Length);

            GCHandle handle = GCHandle.Alloc(motionBuffer, GCHandleType.Pinned);
            try
            {
                return (F1CarMotionData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(F1CarMotionData))!;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Parses telemetry data for player car from packet.
        /// </summary>
        private F1CarTelemetryData ParseTelemetryData(byte[] data, int carIndex)
        {
            // Telemetry data is typically after motion data in compound packets
            // For simplified parsing, we extract from expected offset
            int motionSectionSize = F1_MAX_CARS * Marshal.SizeOf(typeof(F1CarMotionData));
            int telemetryOffset = F1_PACKET_HEADER_SIZE + motionSectionSize + 
                (carIndex * Marshal.SizeOf(typeof(F1CarTelemetryData)));
            
            if (telemetryOffset + Marshal.SizeOf(typeof(F1CarTelemetryData)) > data.Length)
            {
                // Return default if not available in packet
                return new F1CarTelemetryData();
            }

            byte[] telemetryBuffer = new byte[Marshal.SizeOf(typeof(F1CarTelemetryData))];
            Array.Copy(data, telemetryOffset, telemetryBuffer, 0, telemetryBuffer.Length);

            GCHandle handle = GCHandle.Alloc(telemetryBuffer, GCHandleType.Pinned);
            try
            {
                return (F1CarTelemetryData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(F1CarTelemetryData))!;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Parses tire data from F1 24 telemetry.
        /// </summary>
        private TireData ParseF1TireData(F1CarTelemetryData telemetry)
        {
            // Tire positions: 0=FL, 1=FR, 2=BL, 3=BR
            return new TireData
            {
                FrontLeft = CreateF1TireInfo(telemetry, 0),
                FrontRight = CreateF1TireInfo(telemetry, 1),
                BackLeft = CreateF1TireInfo(telemetry, 2),
                BackRight = CreateF1TireInfo(telemetry, 3)
            };
        }

        /// <summary>
        /// Creates TireInfo for single F1 tire.
        /// </summary>
        private TireInfo CreateF1TireInfo(F1CarTelemetryData telemetry, int tireIndex)
        {
            // F1 tire pressure is stored as PSI * 10
            float pressurePsi = telemetry.TiresPressure[tireIndex] / 10f;
            float pressureKpa = pressurePsi * 6.89476f;

            return new TireInfo
            {
                Temperature = Clamp(telemetry.TiresSurfaceTemperature[tireIndex], 0, 150),
                Wear = 0f, // F1 doesn't expose tire wear directly
                Pressure = Math.Max(pressureKpa, 0),
                Load = 0f, // Not exposed in basic telemetry
                SuspensionTravel = 0f,
                SuspensionVelocity = 0f
            };
        }

        /// <summary>
        /// Maps F1 gear values to standard format.
        /// F1: -1=Reverse, 0=Neutral, 1-N=Forward
        /// </summary>
        private int MapF1Gear(sbyte f1Gear)
        {
            if (f1Gear <= -1) return -1; // Reverse
            if (f1Gear == 0) return 0;   // Neutral
            return Math.Max(f1Gear, 1);  // Forward gears
        }

        /// <summary>
        /// Converts G-force values properly.
        /// </summary>
        private float ConvertGForce(float accel)
        {
            // F1 provides m/s^2, convert to G (1G = 9.81 m/s^2)
            return accel / 9.81f;
        }

        /// <summary>
        /// Determines session state from F1 header.
        /// </summary>
        private string GetSessionState(F1PacketHeader header)
        {
            return header.PausePacket == 0 ? "Running" : "Paused";
        }

        /// <summary>
        /// Clamps value between min and max.
        /// </summary>
        private float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Records parsing errors.
        /// </summary>
        private void RecordParsingError(string error)
        {
            System.Console.WriteLine($"[F1 24 Parser] {error}");
        }

        /// <summary>
        /// Increments success counter for statistics.
        /// </summary>
        private void IncrementSuccessCount()
        {
            // TODO: Update telemetry statistics
        }
    }
}
