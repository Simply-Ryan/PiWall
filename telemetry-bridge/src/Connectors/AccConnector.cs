using System;
using System.Runtime.InteropServices;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Connectors
{
    /// <summary>
    /// ACC (Assetto Corsa Competizione) telemetry UDP connector implementation.
    /// Parses binary telemetry packets from ACC sim running on localhost:9996
    /// ACC sends ~100 Hz telemetry updates (~10ms intervals)
    /// 
    /// ACC uses 3 separate UDP packet types:
    /// 1. Physics packet: vehicle dynamics, tire data, fuel
    /// 2. Graphics packet: session info, lap times, positions
    /// 3. StaticInfo packet: track/car info (less frequent)
    /// 
    /// This connector handles the Physics packet as primary telemetry source.
    /// </summary>
    public class AccConnector : BaseSimConnector
    {
        // ACC UDP constants
        private const int ACC_PHYSICS_HEADER_SIZE = 12;
        private const int ACC_MAX_PACKET_SIZE = 1024;
        
        // Gear constants  
        private const int ACC_GEAR_REVERSE = -1;
        private const int ACC_GEAR_NEUTRAL = 0;
        
        // UDP port for ACC telemetry (physics packets)
        public override int DefaultPort => 9996;
        public override string SimulatorName => "ACC";

        /// <summary>
        /// ACC Physics packet header structure.
        /// All fields are little-endian format.
        /// Sent at ~100 Hz (10ms intervals)
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct AccPhysicsHeader
        {
            public int PacketID;              // Sequential packet ID
            public int BrakeBias;             // Brake bias percentage (0-100)
            public float LocalAngularVelocity; // Local angular velocity
        }

        /// <summary>
        /// ACC Physics telemetry data structure (partial - key fields).
        /// ACC Physics packet is ~1100+ bytes total with comprehensive data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct AccPhysicsTelemetry
        {
            // Speed and motion
            public float Speed;              // Speed in m/s
            public float AccG;               // Acceleration in G (magnitude of acceleration vector)
            public float Slip;               // Slip ratio
            
            // Controls
            public float Throttle;           // Throttle position (0.0-1.0)
            public float Brake;              // Brake pressure (0.0-1.0)
            public float Clutch;             // Clutch position (0.0-1.0)
            public float SteerAngle;         // Steering wheel angle in radians
            public int Gear;                 // Gear (-1=Reverse, 0=Neutral, 1-N=Forward)
            
            // Engine
            public float RPM;                // Engine RPM
            public float FuelRemaining;      // Fuel remaining in liters
            public float FuelMaxLoad;        // Fuel tank capacity in liters
            public float EngineThrottle;     // Engine throttle (0.0-1.0)
            public float EngineLoad;         // Engine load percentage
            public float WaterTemp;          // Water temperature in celsius
            public float OilTemp;            // Oil temperature in celsius
            public float OilPressure;        // Oil pressure in bar
            
            // Tire data
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireTempCelsius;  // Tire temperature (FL, FR, BL, BR)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireLoad;         // Tire load
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireWear;         // Tire wear percentage (0.0-1.0)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TireSlip;         // Tire slip
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TirePressure;     // Tire pressure in PSI
            
            // Suspension
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionTravel; // Suspension travel (FL, FR, BL, BR)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionVelocity; // Suspension velocity
            
            // Aerodynamics
            public float FrontWingDamage;    // Front wing damage percentage
            public float RearWingDamage;     // Rear wing damage percentage
            public float DragCoefficient;    // Drag coefficient
            
            // Motion vectors
            public float Lat;                // Lateral acceleration in G
            public float Lon;                // Longitudinal acceleration in G
            public float Vert;               // Vertical acceleration in G
            public float Roll;               // Roll angle in radians
            public float Pitch;              // Pitch angle in radians
            public float Yaw;                // Yaw angle in radians
            
            // Additional telemetry
            public float Drs;                // DRS state/position
            public float TcIn;               // Traction control input
            public float TcInAbs;            // Traction control in ABS
            public float AbsIn;              // ABS input
        }

        /// <summary>
        /// Parses ACC binary telemetry physics packet into UnifiedTelemetryData.
        /// 
        /// ACC sends multiple packet types (Physics, Graphics, StaticInfo).
        /// This parser handles Physics packets which contain vehicle telemetry.
        /// 
        /// Packet size: ~1100 bytes
        /// Update rate: ~100 Hz (10ms intervals)
        /// Performance target: Sub-1ms parsing latency
        /// </summary>
        /// <param name="rawData">Raw UDP packet bytes from ACC</param>
        /// <returns>Parsed UnifiedTelemetryData or null if parsing fails</returns>
        public override UnifiedTelemetryData? Parse(byte[] rawData)
        {
            try
            {
                // Validate packet size
                if (rawData == null || rawData.Length < ACC_PHYSICS_HEADER_SIZE)
                {
                    RecordParsingError($"Invalid ACC packet size: {rawData?.Length ?? 0}");
                    return null;
                }

                // Parse header
                AccPhysicsHeader header = ParseHeader(rawData);

                // Parse physics telemetry
                AccPhysicsTelemetry physics = ParsePhysics(rawData, ACC_PHYSICS_HEADER_SIZE);

                // Build unified telemetry data
                var unifiedData = new UnifiedTelemetryData
                {
                    // Session data
                    SessionData = new SessionData
                    {
                        SimulatorName = SimulatorName,
                        SessionID = header.PacketID.ToString(),
                        SessionNumber = 0, // ACC provides this in graphics packet
                        SessionTime = 0, // ACC provides this in graphics packet
                        SessionTickCount = header.PacketID,
                        SimSpeed = 1f, // ACC doesn't provide pause/speed multiplier in physics
                        SessionStatus = "Active", // ACC provides status in graphics packet
                        SessionState = "Unknown" // Requires graphics packet
                    },

                    // Vehicle data
                    VehicleData = new VehicleData
                    {
                        Speed = ConvertMsToKmh(physics.Speed),
                        Gear = physics.Gear,
                        RPM = (int)physics.RPM,
                        ThrottlePosition = Clamp(physics.Throttle, 0, 1),
                        BrakePosition = Clamp(physics.Brake, 0, 1),
                        ClutchPosition = Clamp(physics.Clutch, 0, 1),
                        FuelAmount = Clamp(physics.FuelRemaining, 0, physics.FuelMaxLoad),
                        FuelCapacity = Math.Max(physics.FuelMaxLoad, 0),
                        WaterTemperature = physics.WaterTemp,
                        OilTemperature = physics.OilTemp,
                        OilPressure = physics.OilPressure,
                        EngineThrottle = Clamp(physics.EngineThrottle, 0, 1)
                    },

                    // Input data
                    InputData = new InputData
                    {
                        SteeringAngle = physics.SteerAngle,
                        Throttle = Clamp(physics.Throttle, 0, 1),
                        Brake = Clamp(physics.Brake, 0, 1),
                        Clutch = Clamp(physics.Clutch, 0, 1),
                        HandBrake = 0f  // Not available in ACC physics
                    },

                    // Tire data
                    TireData = ParseTireData(physics),

                    // Performance data
                    PerformanceData = new PerformanceData
                    {
                        LateralAcceleration = Clamp(physics.Lat, -5, 5),
                        LongitudinalAcceleration = Clamp(physics.Lon, -5, 5),
                        VerticalAcceleration = Clamp(physics.Vert, -5, 5),
                        // Lap times are in graphics packet, not physics
                        CurrentLapTime = null,
                        BestLapTime = null,
                        LastLapTime = null,
                        EstimatedLapTime = null,
                        LapCount = 0,
                        BestLapNumber = 0
                    },

                    // Environment data
                    EnvironmentData = new EnvironmentData
                    {
                        RollAngle = physics.Roll,
                        PitchAngle = physics.Pitch,
                        YawAngle = physics.Yaw,
                        ForceFeedbackIntensity = 0f, // Not in physics packet
                        TrackTemperature = null, // In graphics or static packet
                        AmbientTemperature = null  // In graphics packet
                    },

                    // Set timestamp to now
                    Timestamp = DateTime.UtcNow,
                    ReceivedAt = DateTime.UtcNow
                };

                IncrementSuccessCount();
                return unifiedData;
            }
            catch (Exception ex)
            {
                RecordParsingError($"ACC parsing exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses ACC physics header from raw packet data.
        /// </summary>
        private AccPhysicsHeader ParseHeader(byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return (AccPhysicsHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(AccPhysicsHeader))!;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Parses ACC physics telemetry data from packet payload.
        /// </summary>
        private AccPhysicsTelemetry ParsePhysics(byte[] data, int offset)
        {
            byte[] physicsBuffer = new byte[Marshal.SizeOf(typeof(AccPhysicsTelemetry))];
            int copySize = Math.Min(physicsBuffer.Length, data.Length - offset);
            Array.Copy(data, offset, physicsBuffer, 0, copySize);

            GCHandle handle = GCHandle.Alloc(physicsBuffer, GCHandleType.Pinned);
            try
            {
                return (AccPhysicsTelemetry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(AccPhysicsTelemetry))!;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Parses tire data from ACC telemetry (4 tires: FL, FR, BL, BR).
        /// </summary>
        private TireData ParseTireData(AccPhysicsTelemetry physics)
        {
            // Tire positions: 0=FL, 1=FR, 2=BL, 3=BR
            return new TireData
            {
                FrontLeft = CreateTireInfo(physics, 0),
                FrontRight = CreateTireInfo(physics, 1),
                BackLeft = CreateTireInfo(physics, 2),
                BackRight = CreateTireInfo(physics, 3)
            };
        }

        /// <summary>
        /// Creates TireInfo for a single tire.
        /// </summary>
        private TireInfo CreateTireInfo(AccPhysicsTelemetry physics, int tireIndex)
        {
            // Convert tire pressure from PSI to kPa (ACC uses PSI)
            float pressureKpa = physics.TirePressure[tireIndex] * 6.89476f;

            return new TireInfo
            {
                Temperature = Clamp(physics.TireTempCelsius[tireIndex], -50, 150),
                Wear = Clamp(physics.TireWear[tireIndex], 0, 1),
                Pressure = Math.Max(pressureKpa, 0),
                Load = Math.Max(physics.TireLoad[tireIndex], 0),
                SuspensionTravel = physics.SuspensionTravel[tireIndex],
                SuspensionVelocity = physics.SuspensionVelocity[tireIndex]
            };
        }

        /// <summary>
        /// Converts speed from m/s to km/h.
        /// </summary>
        private float ConvertMsToKmh(float msPerSec)
        {
            return msPerSec * 3.6f;
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
            System.Console.WriteLine($"[ACC Parser] {error}");
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
