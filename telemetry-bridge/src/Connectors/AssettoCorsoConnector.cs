using System;
using System.Runtime.InteropServices;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Connectors
{
    /// <summary>
    /// Assetto Corsa native UDP telemetry connector implementation.
    /// Parses binary telemetry packets from Assetto Corsa (non-Competizione) sim on localhost:10000
    /// AC sends ~60 Hz telemetry updates (~16.67ms intervals)
    /// 
    /// AC uses single UDP packet type containing physics and session data.
    /// Packet is fixed-size binary structure sent continuously while in active session.
    /// </summary>
    public class AssettoCorsoConnector : BaseSimConnector
    {
        // AC UDP constants
        private const int AC_PACKET_SIZE = 328; // Fixed packet size for AC telemetry
        private const int AC_HEADER_OFFSET = 0;
        
        // Gear constants
        private const int AC_GEAR_REVERSE = -1;
        private const int AC_GEAR_NEUTRAL = 0;
        
        // UDP port for AC native telemetry
        public override int DefaultPort => 10000;
        public override string SimulatorName => "Assetto Corsa";

        /// <summary>
        /// AC native telemetry packet structure.
        /// Contains physics, session, and vehicle data.
        /// Total size: 328 bytes
        /// All fields are little-endian format.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct AcTelemetry
        {
            // Packet header
            public int PacketID;              // Packet sequence ID

            // Vehicle position and motion (3D vectors)
            public float Posx, Posy, Posz;    // Position in world coordinates
            public float Velx, Vely, Velz;    // Velocity vector
            public float Accx, Accy, Accz;    // Acceleration vector

            // Rotation (radians)
            public float RotX, RotY, RotZ;    // Euler angles: roll, pitch, yaw

            // Speed and drivetrain
            public float SpeedKmh;            // Speed in km/h
            public float Throttle;            // Throttle input (0.0-1.0)
            public float Brake;               // Brake input (0.0-1.0)
            public float Clutch;              // Clutch input (0.0-1.0)
            public float Gear;                // Current gear
            public float RPM;                 // Engine RPM
            public float SteerAngle;          // Steering wheel angle (radians)

            // Lap and session info
            public int Lap;                   // Current lap number
            public float LapTime;             // Current lap time (ms), or negative if invalid
            public float LastLapTime;         // Last lap time (ms)
            public float BestLapTime;         // Best lap time (ms)
            public int CompletedLaps;         // Number of completed laps
            public int Position;              // Player position in race
            public int MaxPosition;           // Total players in session
            public float Fuel;                // Fuel remaining (liters)
            public int DrsAvailable;          // DRS available (if applicable)
            public int DrsEngaged;            // DRS engaged (if applicable)

            // Tire data (4 wheels: FL, FR, BL, BR)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireWear;          // Tire wear percentage (0.0-1.0)
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireLoad;          // Tire load (normalized)
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireTemp;          // Tire temperature (celsius)
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TirePressure;      // Tire pressure (PSI)
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionTravel;  // Suspension travel
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionVelocity; // Suspension velocity

            // Engine and cooling
            public float WaterTemp;           // Water temperature (celsius)
            public float OilTemp;             // Oil temperature (celsius)
            public float OilPressure;         // Oil pressure (bar)

            // Aerodynamics and damage
            public float AeroDamage;          // Aerodynamic damage ratio
            public float EngineDamage;        // Engine damage ratio

            // Additional flags
            public int IsAiControlled;        // Is vehicle AI controlled
            public int IsAbsActive;           // Is ABS active
            public int IsTcActive;            // Is traction control active
            public float TcLevel;             // TC level (0-1)
            public int IsSpinning;            // Is vehicle spinning
            public float IsSkidding;          // Skid factor
        }

        /// <summary>
        /// Parses AC native binary telemetry packet into UnifiedTelemetryData.
        /// 
        /// AC sends fixed-size (328 byte) UDP packets at ~60 Hz.
        /// Packet structure is simpler than ACC/iRacing but contains essential telemetry.
        /// 
        /// Performance target: Sub-1.5ms parsing latency
        /// </summary>
        /// <param name="rawData">Raw UDP packet bytes from Assetto Corsa</param>
        /// <returns>Parsed UnifiedTelemetryData or null if parsing fails</returns>
        public override UnifiedTelemetryData? Parse(byte[] rawData)
        {
            try
            {
                // Validate packet size - AC packets are fixed size
                if (rawData == null || rawData.Length < AC_PACKET_SIZE)
                {
                    RecordParsingError($"Invalid AC packet size: {rawData?.Length ?? 0}, expected: {AC_PACKET_SIZE}");
                    return null;
                }

                // Parse telemetry structure
                AcTelemetry telemetry = ParseTelemetry(rawData);

                // Build unified telemetry data
                var unifiedData = new UnifiedTelemetryData
                {
                    // Session data
                    SessionData = new SessionData
                    {
                        SimulatorName = SimulatorName,
                        SessionID = telemetry.PacketID.ToString(),
                        SessionNumber = 0,
                        SessionTime = Math.Max(telemetry.LapTime, 0) / 1000f, // Convert ms to seconds
                        SessionTickCount = telemetry.PacketID,
                        SimSpeed = 1f,
                        SessionStatus = GetSessionStatus(telemetry),
                        SessionState = GetSessionState(telemetry)
                    },

                    // Vehicle data
                    VehicleData = new VehicleData
                    {
                        Speed = telemetry.SpeedKmh,
                        Gear = (int)telemetry.Gear,
                        RPM = (int)telemetry.RPM,
                        ThrottlePosition = Clamp(telemetry.Throttle, 0, 1),
                        BrakePosition = Clamp(telemetry.Brake, 0, 1),
                        ClutchPosition = Clamp(telemetry.Clutch, 0, 1),
                        FuelAmount = Math.Max(telemetry.Fuel, 0),
                        FuelCapacity = 0, // AC doesn't provide fuel capacity
                        WaterTemperature = telemetry.WaterTemp,
                        OilTemperature = telemetry.OilTemp,
                        OilPressure = telemetry.OilPressure
                    },

                    // Input data
                    InputData = new InputData
                    {
                        SteeringAngle = telemetry.SteerAngle,
                        Throttle = Clamp(telemetry.Throttle, 0, 1),
                        Brake = Clamp(telemetry.Brake, 0, 1),
                        Clutch = Clamp(telemetry.Clutch, 0, 1),
                        HandBrake = 0f
                    },

                    // Tire data
                    TireData = ParseTireData(telemetry),

                    // Performance data
                    PerformanceData = new PerformanceData
                    {
                        LateralAcceleration = Clamp(telemetry.Accy, -5, 5),
                        LongitudinalAcceleration = Clamp(telemetry.Accx, -5, 5),
                        VerticalAcceleration = Clamp(telemetry.Accz, -5, 5),
                        CurrentLapTime = telemetry.LapTime > 0 ? telemetry.LapTime / 1000f : null,
                        BestLapTime = telemetry.BestLapTime > 0 ? telemetry.BestLapTime / 1000f : null,
                        LastLapTime = telemetry.LastLapTime > 0 ? telemetry.LastLapTime / 1000f : null,
                        EstimatedLapTime = null,
                        LapCount = telemetry.Lap,
                        BestLapNumber = 0
                    },

                    // Environment data
                    EnvironmentData = new EnvironmentData
                    {
                        RollAngle = telemetry.RotX,
                        PitchAngle = telemetry.RotY,
                        YawAngle = telemetry.RotZ,
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
                RecordParsingError($"AC parsing exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses AC telemetry structure from raw packet data.
        /// </summary>
        private AcTelemetry ParseTelemetry(byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return (AcTelemetry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(AcTelemetry))!;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Determines session status from AC telemetry.
        /// </summary>
        private string GetSessionStatus(AcTelemetry telemetry)
        {
            if (telemetry.LapTime < 0)
                return "Invalid";
            
            if (telemetry.IsSpinning != 0)
                return "Spinning";
            
            if (Math.Abs(telemetry.IsSkidding) > 0.5f)
                return "Skidding";
            
            return "Active";
        }

        /// <summary>
        /// Determines session state from AC telemetry.
        /// </summary>
        private string GetSessionState(AcTelemetry telemetry)
        {
            if (telemetry.IsAiControlled != 0)
                return "AI";
            
            return "Player";
        }

        /// <summary>
        /// Parses tire data from AC telemetry (4 tires: FL, FR, BL, BR).
        /// </summary>
        private TireData ParseTireData(AcTelemetry telemetry)
        {
            // Tire positions: 0=FL, 1=FR, 2=BL, 3=BR
            return new TireData
            {
                FrontLeft = CreateTireInfo(telemetry, 0),
                FrontRight = CreateTireInfo(telemetry, 1),
                BackLeft = CreateTireInfo(telemetry, 2),
                BackRight = CreateTireInfo(telemetry, 3)
            };
        }

        /// <summary>
        /// Creates TireInfo for a single tire.
        /// </summary>
        private TireInfo CreateTireInfo(AcTelemetry telemetry, int tireIndex)
        {
            // Convert tire pressure from PSI to kPa
            float pressureKpa = telemetry.TirePressure[tireIndex] * 6.89476f;

            return new TireInfo
            {
                Temperature = Clamp(telemetry.TireTemp[tireIndex], -50, 150),
                Wear = Clamp(telemetry.TireWear[tireIndex], 0, 1),
                Pressure = Math.Max(pressureKpa, 0),
                Load = Math.Max(telemetry.TireLoad[tireIndex], 0),
                SuspensionTravel = telemetry.SuspensionTravel[tireIndex],
                SuspensionVelocity = telemetry.SuspensionVelocity[tireIndex]
            };
        }

        /// <summary>
        /// Clamps value between min and max bounds.
        /// </summary>
        private float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Records parsing errors for debugging.
        /// </summary>
        private void RecordParsingError(string error)
        {
            System.Console.WriteLine($"[AC Parser] {error}");
        }

        /// <summary>
        /// Increments successful parse counter.
        /// </summary>
        private void IncrementSuccessCount()
        {
            // TODO: Update telemetry statistics
        }
    }
}
