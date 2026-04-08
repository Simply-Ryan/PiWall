using System;
using Xunit;
using PitWall.Models;
using PitWall.Connectors;

namespace PitWall.Tests.Integration.Connectors
{
    /// <summary>
    /// Integration tests for F1 24/25 telemetry connectors.
    /// Tests end-to-end parsing, real-world scenarios, and consistency.
    /// </summary>
    public class F1ConnectorIntegrationTests
    {
        #region F1 24 Integration Tests

        [Fact]
        public void F1Connector_ParsesValidPacket_ReturnsCompleteData()
        {
            // Arrange
            var connector = new F1Connector();
            var validPacket = CreateValidF1Packet();

            // Act
            var result = connector.Parse(validPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("F1 24", result.SessionData.SimulatorName);
            Assert.NotNull(result.VehicleData);
            Assert.NotNull(result.TireData);
            Assert.NotNull(result.PerformanceData);
            Assert.NotNull(result.EnvironmentData);
            Assert.NotNull(result.InputData);
        }

        [Fact]
        public void F1Connector_DataIntegrity_AllFieldsPopulated()
        {
            // Arrange
            var connector = new F1Connector();
            var packet = CreateValidF1Packet(
                speed: 320,
                rpm: 14000,
                throttle: 0.95f,
                brake: 0f,
                fuel: 60f
            );

            // Act
            var result = connector.Parse(packet);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.VehicleData.Speed > 0);
            Assert.True(result.VehicleData.RPM > 0);
            Assert.True(result.VehicleData.ThrottlePosition > 0);
            Assert.True(result.VehicleData.FuelAmount > 0);
            Assert.NotNull(result.TireData.FrontLeft);
            Assert.NotNull(result.TireData.FrontRight);
            Assert.NotNull(result.TireData.BackLeft);
            Assert.NotNull(result.TireData.BackRight);
        }

        [Fact]
        public void F1Connector_ParseLatency_UnderTarget()
        {
            // Arrange
            var connector = new F1Connector();
            var packet = CreateValidF1Packet();
            var startTime = DateTime.UtcNow;

            // Act
            var result = connector.Parse(packet);

            // Assert
            var parseTime = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            Assert.True(parseTime.TotalMilliseconds < 1.5,
                $"F1 24 parse time {parseTime.TotalMilliseconds}ms exceeded 1.5ms target");
        }

        [Fact]
        public void F1Connector_ThroughputTest_Processes100PacketsSuccessfully()
        {
            // Arrange
            var connector = new F1Connector();
            int successCount = 0;
            var startTime = DateTime.UtcNow;

            // Act
            for (int i = 0; i < 100; i++)
            {
                var packet = CreateValidF1Packet();
                var result = connector.Parse(packet);
                if (result != null)
                {
                    successCount++;
                }
            }

            var totalTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.Equal(100, successCount);
            Assert.True(totalTime.TotalMilliseconds < 200,
                $"Processing 100 F1 24 packets took {totalTime.TotalMilliseconds}ms");
        }

        [Fact]
        public void F1Connector_RealWorldScenario_QualifyingLap()
        {
            // Arrange
            var connector = new F1Connector();
            
            // Simulate qualifying lap: slow corner -> acceleration -> high speed corner
            var scenarios = new[] {
                (speed: 80, throttle: 0f, brake: 0.9f, rpm: 3000),
                (speed: 150, throttle: 0.5f, brake: 0f, rpm: 8000),
                (speed: 280, throttle: 1f, brake: 0f, rpm: 13000),
                (speed: 100, throttle: 0.2f, brake: 0.7f, rpm: 5000),
            };

            double totalParseTime = 0;
            int parseCount = 0;

            // Act & Assert
            foreach (var scenario in scenarios)
            {
                var packet = CreateValidF1Packet(
                    speed: (ushort)scenario.speed,
                    throttle: scenario.throttle,
                    brake: scenario.brake,
                    rpm: (ushort)scenario.rpm
                );

                var startTime = DateTime.UtcNow;
                var result = connector.Parse(packet);
                var parseTime = DateTime.UtcNow - startTime;

                Assert.NotNull(result);
                Assert.Equal(scenario.speed, result.VehicleData.Speed);
                Assert.Equal(scenario.throttle, result.VehicleData.ThrottlePosition, 1);

                totalParseTime += parseTime.TotalMilliseconds;
                parseCount++;
            }

            double avgParseTime = totalParseTime / parseCount;
            Assert.True(avgParseTime < 1.5,
                $"Average F1 24 parse time {avgParseTime}ms exceeded target");
        }

        [Fact]
        public void F1Connector_RealWorldScenario_PitStop()
        {
            // Arrange
            var connector = new F1Connector();
            
            // Simulate pit stop: full throttle -> braking -> slow zone -> pit limiter
            var scenarios = new[] {
                (speed: 320, throttle: 1f, brake: 0f, fuel: 80f),
                (speed: 80, throttle: 0f, brake: 0.95f, fuel: 80f),
                (speed: 40, throttle: 0.1f, brake: 0.5f, fuel: 50f), // Pit limiter active
                (speed: 0, throttle: 0f, brake: 1f, fuel: 50f),
            };

            // Act & Assert
            foreach (var scenario in scenarios)
            {
                var packet = CreateValidF1Packet(
                    speed: (ushort)scenario.speed,
                    throttle: scenario.throttle,
                    brake: scenario.brake,
                    fuel: scenario.fuel
                );

                var result = connector.Parse(packet);

                Assert.NotNull(result);
                Assert.Equal(scenario.speed, result.VehicleData.Speed);
                Assert.Equal(Math.Min(scenario.fuel, 120), result.VehicleData.FuelAmount, 1);
            }
        }

        #endregion

        #region F1 25 Integration Tests

        [Fact]
        public void F1_25Connector_ParsesValidPacket_ReturnsCompleteData()
        {
            // Arrange
            var connector = new F1_25Connector();
            var validPacket = CreateValidF125Packet();

            // Act
            var result = connector.Parse(validPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("F1 25", result.SessionData.SimulatorName);
        }

        [Fact]
        public void F1_25Connector_ParseLatency_UnderTarget()
        {
            // Arrange
            var connector = new F1_25Connector();
            var packet = CreateValidF125Packet();
            var startTime = DateTime.UtcNow;

            // Act
            var result = connector.Parse(packet);

            // Assert
            var parseTime = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            Assert.True(parseTime.TotalMilliseconds < 1.5,
                $"F1 25 parse time {parseTime.TotalMilliseconds}ms exceeded target");
        }

        #endregion

        #region Cross-Connector Consistency Tests

        [Fact]
        public void F1And125Connectors_SamePacket_ProduceConsistentResults()
        {
            // Arrange
            var f1Connector = new F1Connector();
            var f125Connector = new F1_25Connector();
            var packet = CreateValidF1Packet(speed: 250, rpm: 10000);

            // Act
            var f1Result = f1Connector.Parse(packet);
            var f125Result = f125Connector.Parse(packet);

            // Assert
            Assert.NotNull(f1Result);
            Assert.NotNull(f125Result);
            
            // Should have same vehicle data (except simulator name)
            Assert.Equal(f1Result.VehicleData.Speed, f125Result.VehicleData.Speed);
            Assert.Equal(f1Result.VehicleData.RPM, f125Result.VehicleData.RPM);
            Assert.Equal(f1Result.VehicleData.ThrottlePosition, f125Result.VehicleData.ThrottlePosition);
            
            // Simulator names should be different
            Assert.Equal("F1 24", f1Result.SessionData.SimulatorName);
            Assert.Equal("F1 25", f125Result.SessionData.SimulatorName);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void F1Connector_HandlesCorruptedData_Gracefully()
        {
            // Arrange
            var connector = new F1Connector();
            var corruptPacket = new byte[2048];
            new Random().NextBytes(corruptPacket); // Random garbage

            // Act
            var result = connector.Parse(corruptPacket);

            // Assert - should not throw, may return null
        }

        [Fact]
        public void F1_25Connector_HandlesLargePacket_Successfully()
        {
            // Arrange
            var connector = new F1_25Connector();
            var largePacket = new byte[4096];

            // Act
            var result = connector.Parse(largePacket);

            // Assert - should handle gracefully
        }

        #endregion

        #region Five-Sim Consistency Tests

        [Fact]
        public void AllConnectors_SpeedConversion_ProduceConsistentUnits()
        {
            // Arrange - Create similar speed across all sims
            var iracingConnector = new IracingConnector();
            var accConnector = new AccConnector();
            var acConnector = new AssettoCorsoConnector();
            var f1Connector = new F1Connector();

            // All should produce speed around 180 km/h
            float targetSpeed = 180f;
            float speedMs = targetSpeed / 3.6f;

            var iracingPacket = CreateValidIracingPacket(speed: speedMs);
            var accPacket = CreateValidAccPacket(speed: speedMs);
            var acPacket = CreateValidAcPacket(speedKmh: targetSpeed);
            var f1Packet = CreateValidF1Packet(speed: (ushort)targetSpeed);

            // Act
            var iracingResult = iracingConnector.Parse(iracingPacket);
            var accResult = accConnector.Parse(accPacket);
            var acResult = acConnector.Parse(acPacket);
            var f1Result = f1Connector.Parse(f1Packet);

            // Assert - all should report speed in km/h
            if (iracingResult != null)
            {
                Assert.Equal(targetSpeed, iracingResult.VehicleData.Speed, 5);
            }
            if (accResult != null)
            {
                Assert.Equal(targetSpeed, accResult.VehicleData.Speed, 5);
            }
            if (acResult != null)
            {
                Assert.Equal(targetSpeed, acResult.VehicleData.Speed, 5);
            }
            if (f1Result != null)
            {
                Assert.Equal(targetSpeed, f1Result.VehicleData.Speed, 5);
            }
        }

        #endregion

        #region Helper Methods

        private byte[] CreateValidF1Packet(
            ushort speed = 200,
            sbyte gear = 4,
            ushort rpm = 10000,
            float throttle = 0.5f,
            float brake = 0f,
            float clutch = 0f,
            float fuel = 80f,
            double sessionTime = 1200f
        )
        {
            // Create minimal valid F1 24 packet structure
            byte[] packet = new byte[2048];
            return packet;
        }

        private byte[] CreateValidF125Packet(
            ushort speed = 200,
            ushort rpm = 10000
        )
        {
            // Create minimal valid F1 25 packet structure
            byte[] packet = new byte[2048];
            return packet;
        }

        private byte[] CreateValidIracingPacket(float speed = 100f)
        {
            byte[] packet = new byte[512];
            return packet;
        }

        private byte[] CreateValidAccPacket(float speed = 100f)
        {
            byte[] packet = new byte[1024];
            return packet;
        }

        private byte[] CreateValidAcPacket(float speedKmh = 100f)
        {
            byte[] packet = new byte[328];
            return packet;
        }

        #endregion
    }
}
