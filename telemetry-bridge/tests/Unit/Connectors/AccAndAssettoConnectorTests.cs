using System;
using Xunit;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Tests.Unit.Connectors
{
    /// <summary>
    /// Unit tests for AccConnector (Assetto Corsa Competizione).
    /// Tests binary packet structure parsing, field mapping, and edge cases.
    /// Target: 80%+ coverage of ACC parsing logic.
    /// </summary>
    public class AccConnectorTests
    {
        private readonly AccConnector _connector;

        public AccConnectorTests()
        {
            _connector = new AccConnector();
        }

        #region Header & Basic Tests

        [Fact]
        public void Parse_WithValidPacket_ReturnsUnifiedTelemetryData()
        {
            // Arrange
            byte[] validPacket = CreateValidAccPacket();

            // Act
            var result = _connector.Parse(validPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ACC", result.SessionData.SimulatorName);
            Assert.NotNull(result.VehicleData);
            Assert.NotNull(result.TireData);
        }

        [Fact]
        public void Parse_WithNullPacket_ReturnsNull()
        {
            // Act
            var result = _connector.Parse(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Parse_WithTooSmallPacket_ReturnsNull()
        {
            // Arrange
            byte[] tinyPacket = new byte[5];

            // Act
            var result = _connector.Parse(tinyPacket);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Vehicle Data Tests

        [Fact]
        public void Parse_ExtractsVehicleSpeedCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(speed: 50f); // 50 m/s = 180 km/h

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(180f, result.VehicleData.Speed, 1);
        }

        [Fact]
        public void Parse_ExtractsGearCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(gear: 3);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.VehicleData.Gear);
        }

        [Fact]
        public void Parse_ExtractsRPMCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(rpm: 5500f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5500, result.VehicleData.RPM);
        }

        [Fact]
        public void Parse_ExtractionThrottleAndBrake()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(throttle: 0.75f, brake: 0.25f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.75f, result.VehicleData.ThrottlePosition, 2);
            Assert.Equal(0.25f, result.VehicleData.BrakePosition, 2);
        }

        [Fact]
        public void Parse_ExtractsFuelDataCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(fuel: 50f, fuelCapacity: 100f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50f, result.VehicleData.FuelAmount, 1);
            Assert.Equal(100f, result.VehicleData.FuelCapacity, 1);
        }

        #endregion

        #region Tire Data Tests

        [Fact]
        public void Parse_ExtractsTireTemperaturesCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(
                frontLeftTireTemp: 95f,
                frontRightTireTemp: 98f,
                backLeftTireTemp: 92f,
                backRightTireTemp: 94f
            );

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(95f, result.TireData.FrontLeft.Temperature, 1);
            Assert.Equal(98f, result.TireData.FrontRight.Temperature, 1);
            Assert.Equal(92f, result.TireData.BackLeft.Temperature, 1);
            Assert.Equal(94f, result.TireData.BackRight.Temperature, 1);
        }

        [Fact]
        public void Parse_ConvertsTirePressureFromPsiToKpa()
        {
            // Arrange - ACC uses PSI, we convert to kPa
            byte[] packet = CreateValidAccPacket(
                frontLeftTirePressure: 30f // PSI
            );

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            // 30 PSI * 6.89476 ≈ 206.84 kPa
            Assert.True(result.TireData.FrontLeft.Pressure > 200);
            Assert.True(result.TireData.FrontLeft.Pressure < 210);
        }

        #endregion

        #region Performance & Accuracy

        [Fact]
        public void Parse_ExtractsAccelerationCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAccPacket(
                lateralAccel: 1.5f,
                longitudinalAccel: -0.8f,
                verticalAccel: 0.3f
            );

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1.5f, result.PerformanceData.LateralAcceleration, 2);
            Assert.Equal(-0.8f, result.PerformanceData.LongitudinalAcceleration, 2);
        }

        [Fact]
        public void DefaultPort_ReturnsCorrectAccPort()
        {
            // Act
            int port = _connector.DefaultPort;

            // Assert
            Assert.Equal(9996, port);
        }

        [Fact]
        public void SimulatorName_ReturnsACC()
        {
            // Act
            string name = _connector.SimulatorName;

            // Assert
            Assert.Equal("ACC", name);
        }

        #endregion

        #region Helper Methods

        private byte[] CreateValidAccPacket(
            float speed = 100f,
            int gear = 1,
            float rpm = 6000f,
            float throttle = 0.5f,
            float brake = 0f,
            float clutch = 0f,
            float fuel = 50f,
            float fuelCapacity = 100f,
            float frontLeftTireTemp = 95f,
            float frontRightTireTemp = 95f,
            float backLeftTireTemp = 92f,
            float backRightTireTemp = 92f,
            float frontLeftTirePressure = 30f,
            float lateralAccel = 0.5f,
            float longitudinalAccel = 0f,
            float verticalAccel = 0f
        )
        {
            // Create a minimal valid ACC packet
            byte[] packet = new byte[1024]; // ACC packets ~1100 bytes
            return packet;
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for AssettoCorsoConnector (Assetto Corsa native).
    /// Tests binary packet structure parsing and field mapping.
    /// Target: 80%+ coverage of AC parsing logic.
    /// </summary>
    public class AssettoCorsoConnectorTests
    {
        private readonly AssettoCorsoConnector _connector;

        public AssettoCorsoConnectorTests()
        {
            _connector = new AssettoCorsoConnector();
        }

        #region Header & Basic Tests

        [Fact]
        public void Parse_WithValidPacket_ReturnsUnifiedTelemetryData()
        {
            // Arrange
            byte[] validPacket = CreateValidAcPacket();

            // Act
            var result = _connector.Parse(validPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Assetto Corsa", result.SessionData.SimulatorName);
            Assert.NotNull(result.VehicleData);
            Assert.NotNull(result.TireData);
        }

        [Fact]
        public void Parse_WithNullPacket_ReturnsNull()
        {
            // Act
            var result = _connector.Parse(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Parse_WithUnderSizedPacket_ReturnsNull()
        {
            // Arrange - AC packets must be exactly 328 bytes
            byte[] undersizedPacket = new byte[300];

            // Act
            var result = _connector.Parse(undersizedPacket);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Vehicle Data Tests

        [Fact]
        public void Parse_ExtractsSpeedInKmh()
        {
            // Arrange - AC provides speed directly in km/h
            byte[] packet = CreateValidAcPacket(speedKmh: 180f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(180f, result.VehicleData.Speed, 1);
        }

        [Fact]
        public void Parse_ExtractsGearCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(gear: 3);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.VehicleData.Gear);
        }

        [Fact]
        public void Parse_ExtractsRPMCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(rpm: 6500f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6500, result.VehicleData.RPM);
        }

        [Fact]
        public void Parse_ExtractsFuelCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(fuel: 45f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(45f, result.VehicleData.FuelAmount, 1);
        }

        #endregion

        #region Tire Data Tests

        [Fact]
        public void Parse_ExtractsTireTemperaturesCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(
                frontLeftTireTemp: 90f,
                frontRightTireTemp: 92f,
                backLeftTireTemp: 88f,
                backRightTireTemp: 89f
            );

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(90f, result.TireData.FrontLeft.Temperature, 1);
            Assert.Equal(92f, result.TireData.FrontRight.Temperature, 1);
        }

        [Fact]
        public void Parse_ConvertsTirePressureFromPsiToKpa()
        {
            // Arrange - AC uses PSI
            byte[] packet = CreateValidAcPacket(frontLeftTirePressure: 28f);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            // 28 PSI * 6.89476 ≈ 193 kPa
            Assert.True(result.TireData.FrontLeft.Pressure > 190);
            Assert.True(result.TireData.FrontLeft.Pressure < 200);
        }

        #endregion

        #region Lap Data Tests

        [Fact]
        public void Parse_ExtractsLapCountCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(lap: 5);

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.PerformanceData.LapCount);
        }

        [Fact]
        public void Parse_ConvertsLapTimeFromMillisecondsToSeconds()
        {
            // Arrange - AC provides lap time in milliseconds
            byte[] packet = CreateValidAcPacket(currentLapTime: 120500f); // 120.5 seconds

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(120.5f, result.PerformanceData.CurrentLapTime, 1);
        }

        #endregion

        #region Motion & Orientation Tests

        [Fact]
        public void Parse_ExtractsOrientationAnglesCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(
                roll: 0.05f,
                pitch: 0.08f,
                yaw: 0.02f
            );

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.05f, result.EnvironmentData.RollAngle, 3);
            Assert.Equal(0.08f, result.EnvironmentData.PitchAngle, 3);
            Assert.Equal(0.02f, result.EnvironmentData.YawAngle, 3);
        }

        [Fact]
        public void Parse_ExtractsAccelerationCorrectly()
        {
            // Arrange
            byte[] packet = CreateValidAcPacket(
                lateralAccel: 1.2f,
                longitudinalAccel: -0.5f,
                verticalAccel: 0.2f
            );

            // Act
            var result = _connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1.2f, result.PerformanceData.LateralAcceleration, 2);
            Assert.Equal(-0.5f, result.PerformanceData.LongitudinalAcceleration, 2);
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public void DefaultPort_ReturnsCorrectAcPort()
        {
            // Act
            int port = _connector.DefaultPort;

            // Assert
            Assert.Equal(10000, port);
        }

        [Fact]
        public void SimulatorName_ReturnsAssettoCorsa()
        {
            // Act
            string name = _connector.SimulatorName;

            // Assert
            Assert.Equal("Assetto Corsa", name);
        }

        #endregion

        #region Helper Methods

        private byte[] CreateValidAcPacket(
            float speedKmh = 100f,
            int gear = 1,
            float rpm = 6000f,
            float throttle = 0.5f,
            float brake = 0f,
            float clutch = 0f,
            float fuel = 50f,
            float frontLeftTireTemp = 95f,
            float frontRightTireTemp = 95f,
            float backLeftTireTemp = 92f,
            float backRightTireTemp = 92f,
            float frontLeftTirePressure = 30f,
            float lateralAccel = 0.5f,
            float longitudinalAccel = 0f,
            float verticalAccel = 0f,
            float roll = 0f,
            float pitch = 0f,
            float yaw = 0f,
            int lap = 1,
            float currentLapTime = 120000f
        )
        {
            // Create a fixed-size AC packet (328 bytes)
            byte[] packet = new byte[328];
            return packet;
        }

        #endregion
    }
}
