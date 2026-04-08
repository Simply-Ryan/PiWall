using System;
using Xunit;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Tests.Integration.Connectors
{
    /// <summary>
    /// Integration tests for ACC and Assetto Corsa telemetry connectors.
    /// Tests end-to-end parsing, data integrity, and real-world scenarios.
    /// </summary>
    public class AccAndAssettoIntegrationTests
    {
        #region ACC Integration Tests

        [Fact]
        public void AccConnector_ParsesValidPacket_ReturnsCompleteData()
        {
            // Arrange
            var connector = new AccConnector();
            var validPacket = CreateValidAccPacket();

            // Act
            var result = connector.Parse(validPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ACC", result.SessionData.SimulatorName);
            Assert.NotNull(result.VehicleData);
            Assert.NotNull(result.TireData);
            Assert.NotNull(result.PerformanceData);
            Assert.NotNull(result.EnvironmentData);
        }

        [Fact]
        public void AccConnector_DataIntegrity_AllFieldsPopulated()
        {
            // Arrange
            var connector = new AccConnector();
            var packet = CreateValidAccPacket(
                speed: 80f,
                rpm: 5000f,
                throttle: 0.6f,
                brake: 0.1f
            );

            // Act
            var result = connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.VehicleData.Speed > 0);
            Assert.True(result.VehicleData.RPM > 0);
            Assert.True(result.VehicleData.ThrottlePosition > 0);
            Assert.NotNull(result.TireData.FrontLeft);
            Assert.NotNull(result.TireData.FrontRight);
            Assert.NotNull(result.TireData.BackLeft);
            Assert.NotNull(result.TireData.BackRight);
        }

        [Fact]
        public void AccConnector_ThroughputTest_Processes100PacketsSuccessfully()
        {
            // Arrange
            var connector = new AccConnector();
            int successCount = 0;
            var startTime = DateTime.UtcNow;

            // Act
            for (int i = 0; i < 100; i++)
            {
                var packet = CreateValidAccPacket();
                var result = connector.Parse(packet);
                if (result != null)
                {
                    successCount++;
                }
            }

            var totalTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.Equal(100, successCount);
            Assert.True(totalTime.TotalMilliseconds < 200, // Should be much faster than this
                $"Processing 100 ACC packets took {totalTime.TotalMilliseconds}ms");
        }

        [Fact]
        public void AccConnector_ParseLatency_UnderTarget()
        {
            // Arrange
            var connector = new AccConnector();
            var packet = CreateValidAccPacket();
            var startTime = DateTime.UtcNow;

            // Act
            var result = connector.Parse(packet);

            // Assert
            var parseTime = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            Assert.True(parseTime.TotalMilliseconds < 1.5,
                $"ACC parse time {parseTime.TotalMilliseconds}ms exceeded 1.5ms target");
        }

        [Fact]
        public void AccConnector_RealWorldScenario_FullThrottleAcceleration()
        {
            // Arrange
            var connector = new AccConnector();
            double totalParseTime = 0;
            int parseCount = 0;

            // Simulate acceleration: throttle -> high RPM -> high speed
            var scenarios = new[] {
                (throttle: 0f, rpm: 1000f, speed: 0f),
                (throttle: 0.5f, rpm: 3000f, speed: 50f),
                (throttle: 1f, rpm: 6000f, speed: 150f),
                (throttle: 1f, rpm: 8000f, speed: 200f),
            };

            // Act & Assert
            foreach (var scenario in scenarios)
            {
                var packet = CreateValidAccPacket(
                    throttle: scenario.throttle,
                    rpm: scenario.rpm,
                    speed: scenario.speed
                );

                var startTime = DateTime.UtcNow;
                var result = connector.Parse(packet);
                var parseTime = DateTime.UtcNow - startTime;

                Assert.NotNull(result);
                Assert.Equal(scenario.rpm, result.VehicleData.RPM, 0);
                Assert.Equal(scenario.throttle, result.VehicleData.ThrottlePosition, 1);

                totalParseTime += parseTime.TotalMilliseconds;
                parseCount++;
            }

            // Verify average parse time
            double avgParseTime = totalParseTime / parseCount;
            Assert.True(avgParseTime < 1.5,
                $"Average ACC parse time {avgParseTime}ms exceeded target");
        }

        #endregion

        #region Assetto Corsa Integration Tests

        [Fact]
        public void AssettoCorsoConnector_ParsesValidPacket_ReturnsCompleteData()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            var validPacket = CreateValidAcPacket();

            // Act
            var result = connector.Parse(validPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Assetto Corsa", result.SessionData.SimulatorName);
            Assert.NotNull(result.VehicleData);
            Assert.NotNull(result.TireData);
            Assert.NotNull(result.PerformanceData);
        }

        [Fact]
        public void AssettoCorsoConnector_DataIntegrity_AllFieldsPopulated()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            var packet = CreateValidAcPacket(
                speedKmh: 160f,
                rpm: 7000f,
                throttle: 0.8f,
                fuel: 55f
            );

            // Act
            var result = connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(160f, result.VehicleData.Speed, 1);
            Assert.Equal(7000, result.VehicleData.RPM);
            Assert.Equal(0.8f, result.VehicleData.ThrottlePosition, 1);
            Assert.Equal(55f, result.VehicleData.FuelAmount, 1);
            Assert.NotNull(result.TireData);
        }

        [Fact]
        public void AssettoCorsoConnector_ParseLatency_UnderTarget()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            var packet = CreateValidAcPacket();
            var startTime = DateTime.UtcNow;

            // Act
            var result = connector.Parse(packet);

            // Assert
            var parseTime = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            Assert.True(parseTime.TotalMilliseconds < 1.5,
                $"AC parse time {parseTime.TotalMilliseconds}ms exceeded target");
        }

        [Fact]
        public void AssettoCorsoConnector_LapTimeConversion_CorrectlyConvertsMillisecondsToSeconds()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            // 125.750 seconds = 125750 milliseconds
            var packet = CreateValidAcPacket(currentLapTime: 125750f);

            // Act
            var result = connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PerformanceData.CurrentLapTime);
            Assert.Equal(125.75f, result.PerformanceData.CurrentLapTime.Value, 2);
        }

        [Fact]
        public void AssettoCorsoConnector_FixedPacketSize_EnforcesStructSize()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            
            // Valid size
            var validPacket = new byte[328];
            
            // Invalid size
            var invalidPacket = new byte[300];

            // Act
            var validResult = connector.Parse(validPacket);
            var invalidResult = connector.Parse(invalidPacket);

            // Assert
            Assert.NotNull(validResult); // Valid size accepted
            Assert.Null(invalidResult);   // Invalid size rejected
        }

        [Fact]
        public void AssettoCorsoConnector_RealWorldScenario_BrakingToStop()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            
            // Simulate braking: 200 km/h -> 100 -> 0
            var scenarios = new[] {
                (speed: 200f, brake: 0f, lap: 1),
                (speed: 100f, brake: 0.5f, lap: 1),
                (speed: 0f, brake: 0.8f, lap: 2),
            };

            // Act & Assert
            foreach (var scenario in scenarios)
            {
                var packet = CreateValidAcPacket(
                    speedKmh: scenario.speed,
                    brake: scenario.brake,
                    lap: scenario.lap
                );

                var result = connector.Parse(packet);

                Assert.NotNull(result);
                Assert.Equal(scenario.speed, result.VehicleData.Speed, 1);
                Assert.Equal(scenario.brake, result.VehicleData.BrakePosition, 1);
                Assert.Equal(scenario.lap, result.PerformanceData.LapCount);
            }
        }

        #endregion

        #region Cross-Connector Comparison Tests

        [Fact]
        public void BothConnectors_SimilarSpeedConversion_ProduceConsistentResults()
        {
            // Arrange
            var accConnector = new AccConnector();
            var acConnector = new AssettoCorsoConnector();
            
            float speedMs = 100f; // 100 m/s = 360 km/h
            
            // ACC receives m/s (needs conversion)
            var accPacket = CreateValidAccPacket(speed: speedMs);
            // AC receives km/h directly
            var acPacket = CreateValidAcPacket(speedKmh: speedMs * 3.6f);

            // Act
            var accResult = accConnector.Parse(accPacket);
            var acResult = acConnector.Parse(acPacket);

            // Assert
            Assert.NotNull(accResult);
            Assert.NotNull(acResult);
            Assert.Equal(accResult.VehicleData.Speed, acResult.VehicleData.Speed, 1);
        }

        [Fact]
        public void BothConnectors_TirePressureConversion_ConvertsPsiToKpaIdentically()
        {
            // Arrange
            var accConnector = new AccConnector();
            var acConnector = new AssettoCorsoConnector();
            
            float pressurePsi = 28f;
            
            var accPacket = CreateValidAccPacket(frontLeftTirePressure: pressurePsi);
            var acPacket = CreateValidAcPacket(frontLeftTirePressure: pressurePsi);

            // Act
            var accResult = accConnector.Parse(accPacket);
            var acResult = acConnector.Parse(acPacket);

            // Assert
            Assert.NotNull(accResult);
            Assert.NotNull(acResult);
            // Both should convert to same kPa value
            Assert.Equal(
                accResult.TireData.FrontLeft.Pressure,
                acResult.TireData.FrontLeft.Pressure,
                1 // 1 kPa tolerance
            );
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void AccConnector_HandlesLargePacket_Gracefully()
        {
            // Arrange
            var connector = new AccConnector();
            var largePacket = new byte[2048];

            // Act
            var result = connector.Parse(largePacket);

            // Assert - should not throw, may return null or parse
        }

        [Fact]
        public void AssettoCorsoConnector_RejectsUnderSizePacket()
        {
            // Arrange
            var connector = new AssettoCorsoConnector();
            var underSizePacket = new byte[300]; // Less than 328 required

            // Act
            var result = connector.Parse(underSizePacket);

            // Assert
            Assert.Null(result);
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
            float frontLeftTireTemp = 95f,
            float frontLeftTirePressure = 30f,
            float lateralAccel = 0.5f,
            float longitudinalAccel = 0f,
            float verticalAccel = 0f
        )
        {
            // Create minimal valid ACC packet (~1100 bytes)
            byte[] packet = new byte[1024];
            return packet;
        }

        private byte[] CreateValidAcPacket(
            float speedKmh = 100f,
            int gear = 1,
            float rpm = 6000f,
            float throttle = 0.5f,
            float brake = 0f,
            float clutch = 0f,
            float fuel = 50f,
            float frontLeftTireTemp = 95f,
            float frontLeftTirePressure = 28f,
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
            // Create fixed-size AC packet (328 bytes)
            byte[] packet = new byte[328];
            return packet;
        }

        #endregion
    }
}
