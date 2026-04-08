/**
 * Callout Manager
 * 
 * Generates contextual voice callouts based on real-time telemetry data
 * Tracks state to avoid duplicate callouts
 */

import {
  VoiceNotification,
  VoiceNotificationType,
} from '../types/voice';
import { TelemetrySnapshot } from '../types/telemetry';

class CalloutManager {
  private static instance: CalloutManager;
  private lastCalloutTime: Record<VoiceNotificationType, number> = {};
  private calloutCooldown = 500; // milliseconds between same callout type
  private previousState: Partial<TelemetrySnapshot> = {};

  private constructor() {}

  /**
   * Get singleton instance
   */
  static getInstance(): CalloutManager {
    if (!CalloutManager.instance) {
      CalloutManager.instance = new CalloutManager();
    }
    return CalloutManager.instance;
  }

  /**
   * Generate callouts based on telemetry data
   * Returns array of callouts that should be spoken
   */
  generateCallouts(telemetry: TelemetrySnapshot): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];

    // RPM callouts
    const rpmCallouts = this.generateRPMCallouts(telemetry);
    callouts.push(...rpmCallouts);

    // Gear shift callouts
    const gearCallouts = this.generateGearCallouts(telemetry);
    callouts.push(...gearCallouts);

    // Tire temperature callouts
    const tireCallouts = this.generateTireCallouts(telemetry);
    callouts.push(...tireCallouts);

    // Fuel callouts
    const fuelCallouts = this.generateFuelCallouts(telemetry);
    callouts.push(...fuelCallouts);

    // Performance callouts (lap times)
    const perfCallouts = this.generatePerformanceCallouts(telemetry);
    callouts.push(...perfCallouts);

    // Throttle/brake callouts
    const drivingCallouts = this.generateDrivingCallouts(telemetry);
    callouts.push(...drivingCallouts);

    // Update previous state
    this.previousState = { ...telemetry };

    return callouts;
  }

  /**
   * Generate RPM-based callouts
   */
  private generateRPMCallouts(telemetry: TelemetrySnapshot): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];
    const rpm = telemetry.vehicleData.rpm || 0;
    const maxRPM = telemetry.vehicleData.maxRPM || 8000;

    // Redline warning (>90% RPM)
    if (rpm / maxRPM > 0.9) {
      const callout = this.createCallout(
        VoiceNotificationType.REDLINE_WARNING,
        'Approaching redline',
        'high',
        telemetry,
      );
      if (callout) {
        callouts.push(callout);
      }
    }

    return callouts;
  }

  /**
   * Generate gear shift callouts
   */
  private generateGearCallouts(telemetry: TelemetrySnapshot): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];
    const currentGear = telemetry.vehicleData.gear || 'N';
    const previousGear = this.previousState.vehicleData?.gear || 'N';

    if (currentGear === previousGear) {
      return callouts;
    }

    // Determine shift direction
    const gearOrder = ['R', 'N', '1', '2', '3', '4', '5', '6', '7', '8'];
    const currentIndex = gearOrder.indexOf(currentGear);
    const previousIndex = gearOrder.indexOf(previousGear);

    if (currentIndex > previousIndex) {
      // Upshift
      const callout = this.createCallout(
        VoiceNotificationType.UPSHIFT,
        `Upshift to ${currentGear}`,
        'medium',
        telemetry,
      );
      if (callout) {
        callouts.push(callout);
      }
    } else if (currentIndex < previousIndex && currentIndex > 0) {
      // Downshift (avoid if going to neutral/reverse)
      const callout = this.createCallout(
        VoiceNotificationType.DOWNSHIFT,
        `Downshift to ${currentGear}`,
        'medium',
        telemetry,
      );
      if (callout) {
        callouts.push(callout);
      }
    }

    return callouts;
  }

  /**
   * Generate tire temperature callouts
   */
  private generateTireCallouts(telemetry: TelemetrySnapshot): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];
    const tires = telemetry.vehicleData.tireTemperature;

    if (!tires) return callouts;

    // Check each tire
    Object.entries(tires).forEach(([position, temp]) => {
      if (!temp) return;

      const optimalTemp = 85; // Celsius
      const coldThreshold = 60;
      const hotThreshold = 110;

      // Cold tire warning
      if (temp.middle < coldThreshold) {
        const callout = this.createCallout(
          VoiceNotificationType.TIRE_COLD,
          `${position} tire is cold`,
          'medium',
          telemetry,
        );
        if (callout) {
          callouts.push(callout);
        }
      }

      // Hot tire warning
      if (temp.middle > hotThreshold) {
        const callout = this.createCallout(
          VoiceNotificationType.TIRE_HOT,
          `${position} tire overheating`,
          'high',
          telemetry,
        );
        if (callout) {
          callouts.push(callout);
        }
      }
    });

    return callouts;
  }

  /**
   * Generate fuel-based callouts
   */
  private generateFuelCallouts(telemetry: TelemetrySnapshot): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];
    const fuel = telemetry.vehicleData.fuelLevel || 0;
    const fuelCapacity = telemetry.vehicleData.fuelCapacity || 90;
    const fuelPercent = (fuel / fuelCapacity) * 100;

    // Low fuel warning (20%)
    if (fuelPercent <= 20 && (this.previousState.vehicleData?.fuelLevel || 0) > 20) {
      const callout = this.createCallout(
        VoiceNotificationType.LOW_FUEL,
        `Low fuel, ${Math.round(fuel)} liters remaining`,
        'high',
        telemetry,
      );
      if (callout) {
        callouts.push(callout);
      }
    }

    // Critical fuel warning (5%)
    if (fuelPercent <= 5 && (this.previousState.vehicleData?.fuelLevel || 0) > 5) {
      const callout = this.createCallout(
        VoiceNotificationType.FUEL_CRITICAL,
        'Critical fuel, pit immediately',
        'critical',
        telemetry,
      );
      if (callout) {
        callouts.push(callout);
      }
    }

    return callouts;
  }

  /**
   * Generate performance-based callouts (lap time records)
   */
  private generatePerformanceCallouts(
    telemetry: TelemetrySnapshot,
  ): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];
    const currentLapTime = telemetry.lapData.currentLapTime || 0;
    const bestLapTime = telemetry.lapData.bestLapTime || Infinity;

    // New personal best
    if (currentLapTime > 0 && currentLapTime < bestLapTime) {
      const callout = this.createCallout(
        VoiceNotificationType.NEW_PERSONAL_BEST,
        'New personal best lap time',
        'high',
        telemetry,
      );
      if (callout) {
        callouts.push(callout);
      }
    }

    return callouts;
  }

  /**
   * Generate driving style callouts
   */
  private generateDrivingCallouts(telemetry: TelemetrySnapshot): VoiceNotification[] {
    const callouts: VoiceNotification[] = [];
    const throttle = telemetry.vehicleData.throttle || 0;
    const brake = telemetry.vehicleData.brake || 0;

    // Heavy braking detection
    if (brake > 0.8) {
      const prevBrake = this.previousState.vehicleData?.brake || 0;
      if (prevBrake <= 0.8) {
        const callout = this.createCallout(
          VoiceNotificationType.BRAKE_HARD,
          'Hard braking',
          'low',
          telemetry,
        );
        if (callout) {
          callouts.push(callout);
        }
      }
    }

    return callouts;
  }

  /**
   * Create a callout with cooldown check
   */
  private createCallout(
    type: VoiceNotificationType,
    message: string,
    priority: 'low' | 'medium' | 'high' | 'critical',
    context: TelemetrySnapshot,
  ): VoiceNotification | null {
    const now = Date.now();
    const lastTime = this.lastCalloutTime[type] || 0;

    // Skip if still in cooldown
    if (now - lastTime < this.calloutCooldown) {
      return null;
    }

    this.lastCalloutTime[type] = now;

    return {
      id: `${type}-${now}`,
      type,
      message,
      timestamp: now,
      priority,
      spoken: false,
      context: {
        speed: context.vehicleData.speed,
        rpm: context.vehicleData.rpm,
        gear: context.vehicleData.gear,
        fuelLevel: context.vehicleData.fuelLevel,
        lapTime: context.lapData.currentLapTime,
      },
    };
  }

  /**
   * Set callout cooldown
   */
  setCalloutCooldown(ms: number): void {
    this.calloutCooldown = ms;
  }

  /**
   * Reset state (for new session)
   */
  reset(): void {
    this.lastCalloutTime = {};
    this.previousState = {};
  }
}

export default CalloutManager.getInstance();
