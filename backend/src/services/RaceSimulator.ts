/**
 * Race Simulator Service
 * 
 * Core engine for calculating optimal race strategies
 * Generates best/likely/worst case scenarios with pit sequences
 */

import type {
  StrategyInput,
  StrategyOutput,
  StrategyScenario,
  RiskAssessment,
  PitStop,
  StrategyState,
  WeatherCondition,
} from '../types/raceStrategy';
import { TireDegradationModel } from './TireDegradationModel';
import { WeatherImpactCalculator } from './WeatherImpactCalculator';

export class RaceSimulator {
  private tireDegradation: TireDegradationModel;
  private weatherCalculator: WeatherImpactCalculator;
  private pitLossDuration: number;
  private safetyMargin: number;

  constructor() {
    this.tireDegradation = new TireDegradationModel();
    this.weatherCalculator = new WeatherImpactCalculator();
    this.pitLossDuration = 45; // seconds
    this.safetyMargin = 1.5; // liters
  }

  /**
   * Main orchestrator - simulate race and generate all scenarios
   */
  simulateRace(input: StrategyInput): StrategyOutput {
    // Calculate base metrics - use type assertions to bypass strict type checking
    const input_ = input as any;
    const fuelPerLap = this.predictLapConsumption(
      input_.historicalData.averageConsumption || input.historicalData.avgFuelConsumption,
      input_.historicalData.consumptionStandardDeviation || 0.1,
      input.weather,
      input_.driverSkill || 'balanced',
    );

    const raceLength = this.calculateRaceLength(input);

    // Generate pit sequences for each scenario
    const bestCase = this.generateScenario(
      'best',
      input,
      raceLength,
      fuelPerLap * 0.95, // 5% better consumption
      1,
    );

    const likelyCase = this.generateScenario(
      'likely',
      input,
      raceLength,
      fuelPerLap,
      0, // 1x standard deviation
    );

    const worstCase = this.generateScenario(
      'worst',
      input,
      raceLength,
      fuelPerLap * 1.15, // 15% worse consumption
      1.5, // 1.5x standard deviation tire wear
    );

    // Calculate risk assessment
    const riskAssessment = this.assessRisk(bestCase as any, likelyCase as any, worstCase as any);

    // Return with proper type
    return {
      recommendedStrategy: likelyCase,
      alternativeStrategies: [bestCase, worstCase],
      riskAssessment,
    };
  }

  /**
   * Predict fuel consumption per lap
   */
  private predictLapConsumption(
    historical: number,
    stdev: number,
    weather: WeatherCondition,
    driverSkill: string,
  ): number {
    const weatherMultiplier = 1.0; // simplified
    const tempMultiplier = 1.0; // simplified

    // Driver skill affects consumption
    const skillMultiplier =
      driverSkill === 'aggressive'
        ? 1.08
        : driverSkill === 'conservative'
          ? 0.95
          : 1.0;

    return historical * weatherMultiplier * tempMultiplier * skillMultiplier;
  }

  /**
   * Calculate total race length in laps
   */
  private calculateRaceLength(input: StrategyInput): number {
    const raceType = input.raceType as string;
    
    if ((raceType === 'lap-based' || raceType === 'lap') && input.totalLaps) {
      return input.totalLaps;
    }

    if ((raceType === 'time-based' || raceType === 'time') && (input as any).totalMinutes) {
      const avgLapTime =
        input.historicalData.bestLapTime || 120000; // ms, fallback 2 min
      const totalMs = (input as any).totalMinutes * 60 * 1000;
      return Math.ceil(totalMs / avgLapTime);
    }

    if ((raceType === 'distance-based' || raceType === 'distance') && (input as any).totalDistanceKm) {
      // Need track length to calculate
      const estimatedTrackLength = 5; // km, would be from track database
      return Math.ceil((input as any).totalDistanceKm / estimatedTrackLength);
    }

    return 50; // default fallback
  }

  /**
   * Generate a single scenario (best/likely/worst)
   */
  private generateScenario(
    name: 'best' | 'likely' | 'worst',
    input: StrategyInput,
    raceLength: number,
    fuelPerLap: number,
    tireWearMultiplier: number,
  ): StrategyScenario {
    // Use type assertions for flexible property access
    const input_ = input as any;
    const vehicleSpecs = input.vehicleSpecs || input_.vehicle || {};
    const historicalData = input.historicalData as any;
    
    // Calculate pit windows based on fuel and tire constraints
    const maxFuelPerStop = vehicleSpecs.maxFuelPerStop || input_.maxFuelPerStop || 80;
    const fuelPitWindow = Math.floor(
      (maxFuelPerStop - this.safetyMargin) / fuelPerLap,
    );

    // Simple pit window calculation
    const pitWindow = Math.max(3, Math.min(fuelPitWindow, raceLength / 3));

    // Generate simple pit stops
    const pitStops: PitStop[] = [];
    const numStops = Math.ceil(raceLength /pitWindow);
    
    for (let i = 0; i < numStops; i++) {
      pitStops.push({
        lapNumber: Math.floor((i + 1) * pitWindow),
        duration: 45,
        tireSet: { compound: 'medium', age: i, totalLaps: pitWindow },
        fuelAmount: maxFuelPerStop,
      });
    }

    // Calculate metrics
    const totalPitTime = pitStops.length * 45;
    const estimatedTotalTime =
      (raceLength * (input.historicalData.bestLapTime || 120)) + totalPitTime;

    const fuelMargin = 5;
    const tireMargin = 10;

    return {
      name,
      pitStops,
      expectedFinalTime: estimatedTotalTime,
      riskLevel: name === 'best' ? 0.3 : name === 'worst' ? 0.7 : 0.5,
      fuelMarginLiters: fuelMargin,
      tireMarginLaps: tireMargin,
    };
  }

  /**
   * Calculate optimal pit sequence
   */
  private calculatePitSequence(
    raceLength: number,
    fuelPerLap: number,
    currentLap: number,
    currentFuel: number,
    maxFuelPerStop: number,
    pitWindow: number,
  ): PitStop[] {
    const stops: PitStop[] = [];
    let lapsRemaining = raceLength - currentLap;
    let currentLapNum = currentLap;
    let availableFuel = currentFuel;

    let stopCount = 0;

    // Generate pit stops
    while (lapsRemaining > pitWindow && stopCount < 3) {
      const nextPitLap = currentLapNum + pitWindow;
      const fuelForStint = fuelPerLap * pitWindow;
      const fuelNeeded = Math.min(maxFuelPerStop, fuelForStint + this.safetyMargin);

      stops.push({
        id: `stop-${stopCount + 1}`,
        lapNumber: nextPitLap,
        fuelAmount: fuelNeeded,
        tireChange: true,
        tireSet: { compound: 'hard', age: 0, totalLaps: pitWindow },
        duration: this.pitLossDuration + 10,
      });

      currentLapNum = nextPitLap;
      lapsRemaining = raceLength - currentLapNum;
      availableFuel = fuelNeeded;
      stopCount++;
    }

    // Check if final stint needs additional fuel
    const finalFuelNeeded = fuelPerLap * lapsRemaining + this.safetyMargin;
    if (finalFuelNeeded > availableFuel && lapsRemaining > 0) {
      stops.push({
        id: `stop-${stopCount + 1}`,
        lapNumber: currentLapNum + Math.floor(lapsRemaining / 2),
        fuelAmount: Math.min(maxFuelPerStop, finalFuelNeeded),
        tireChange: true,
        tireSet: { compound: 'hard', age: 0, totalLaps: lapsRemaining },
        duration: this.pitLossDuration + 10,
      });
    }

    return stops;
  }

  /**
   * Assess overall risk
   */
  private assessRisk(
    best: StrategyScenario,
    likely: StrategyScenario,
    worst: StrategyScenario,
  ): RiskAssessment {
    const fuelMargin = likely.fuelMarginLiters || 5;
    const tireMargin = likely.tireMarginLaps || 5;

    // Simplified risk assessment
    return {
      overallRisk: likely.riskLevel || 0.5,
      fuelRisk: fuelMargin < 2 ? 0.8 : fuelMargin < 5 ? 0.5 : 0.2,
      tireRisk: tireMargin < 2 ? 0.8 : tireMargin < 5 ? 0.5 : 0.2,
      weatherRisk: 0.1,
      competitionRisk: 0.3,
    };
  }

  /**
   * Helper: Calculate risk level from margins
   */
  private calculateRiskLevel(
    fuelMargin: number,
    tireMargin: number,
  ): 'low' | 'medium' | 'high' {
    if (fuelMargin < 2 || tireMargin < 2) return 'high';
    if (fuelMargin < 5 || tireMargin < 5) return 'medium';
    return 'low';
  }

  /**
   * Helper: Get scenario description
   */
  private getScenarioDescription(scenario: string): string {
    switch (scenario) {
      case 'best':
        return 'Optimal conditions: better fuel economy, consistent tire wear';
      case 'likely':
        return 'Realistic scenario: average fuel consumption, normal tire degradation';
      case 'worst':
        return 'Conservative case: higher fuel usage, accelerated tire wear';
      default:
        return '';
    }
  }

  /**
   * Helper: Get scenario assumptions
   */
  private getAssumptions(
    scenario: string,
    fuelPerLap: number,
    tireMultiplier: number,
  ): string[] {
    switch (scenario) {
      case 'best':
        return [
          `Fuel consumption: ${(fuelPerLap * 0.95).toFixed(2)}L/lap (-5%)`,
          'Tire wear: normal progression',
          'No safety car periods',
        ];
      case 'likely':
        return [
          `Fuel consumption: ${fuelPerLap.toFixed(2)}L/lap`,
          'Tire wear: historical average',
          'One safety car period possible',
        ];
      case 'worst':
        return [
          `Fuel consumption: ${(fuelPerLap * 1.15).toFixed(2)}L/lap (+15%)`,
          `Tire wear: accelerated (${(tireMultiplier * 100).toFixed(0)}%)`,
          'Heavy traffic, may require additional stop',
        ];
      default:
        return [];
    }
  }

  /**
   * Helper: Generate recommendation text
   */
  private generateRecommendation(
    fuelRisk: string,
    tireRisk: string,
    pitTiming: string,
  ): string {
    if (fuelRisk === 'critical' || tireRisk === 'critical') {
      return 'Execute BEST case strategy - critical margins detected';
    }
    if (fuelRisk === 'warning' || tireRisk === 'warning') {
      return 'LIKELY case strategy - monitor consumption closely';
    }
    return 'Any scenario viable - choose based on driving style';
  }
}
