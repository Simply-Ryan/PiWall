/**
 * Backend Race Strategy Types
 * 
 * Types for server-side strategy calculation and persistence
 */

export type RaceType = 'sprint' | 'endurance' | 'feature' | 'one-off' | 'lap-based' | 'time-based' | 'distance-based';

export interface PitStop {
  id?: string;
  lapNumber: number;
  duration: number;
  tireSet: TireSet;
  fuelAmount: number;
  tireChange?: boolean;
}

export interface TireSet {
  compound: 'soft' | 'medium' | 'hard';
  age: number;
  totalLaps: number;
}

export interface WeatherCondition {
  temperature: number;
  humidity: number;
  rainfall: number;
  windSpeed: number;
  windDirection: string;
}

export interface VehicleSpecs {
  weight: number;
  fuelCapacity: number;
  tireCount: number;
  enginePower: number;
  maxFuelPerStop?: number;
  tireCompound?: string;
  pitLossTime?: number;
}

export interface HistoricalData {
  avgLapTime: number;
  bestLapTime: number;
  avgFuelConsumption: number;
  avgTireWear: number;
  averageConsumption?: number;
  consumptionStandardDeviation?: number;
  tireWearPerLap?: number;
  fuelConsumedSoFar?: number;
}

export interface StrategyInput {
  raceType: RaceType;
  trackLength: number;
  totalLaps: number;
  currentLap: number;
  weather: WeatherCondition;
  vehicleSpecs: VehicleSpecs;
  vehicle?: VehicleSpecs;
  historicalData: HistoricalData;
  pitLossTime?: number;
  driverSkill?: number;
  totalMinutes?: number;
  totalDistanceKm?: number;
}

export interface StrategyScenario {
  name: string;
  pitStops: PitStop[];
  pitSequence?: PitStop[];
  expectedFinalTime: number;
  riskLevel: number;
  fuelMarginLiters?: number;
  tireMarginLaps?: number;
}

export interface StrategyOutput {
  recommendedStrategy: StrategyScenario;
  alternativeStrategies: StrategyScenario[];
  scenarios?: StrategyScenario[];
  riskAssessment: RiskAssessment;
}

export interface RiskAssessment {
  overallRisk: number;
  fuelRisk: number;
  tireRisk: number;
  weatherRisk: number;
  competitionRisk: number;
}

export interface StrategyAdjustment {
  id?: string;
  reason: string;
  adjustedStrategy: StrategyScenario;
  impact: number;
}

export type StrategyState = {
  currentStrategy: StrategyScenario;
  adjustments: StrategyAdjustment[];
  lastUpdated: Date;
};

export type TelemetryData = {
  timestamp: Date | number;
  speed: number;
  throttle: number;
  brake: number;
  gear: number;
  tireTemp: number[];
  tires?: {
    temp: number[]; 
    wear: number[];
    frontLeft?: { wear: number };
    frontRight?: { wear: number };
    rearLeft?: { wear: number };
    rearRight?: { wear: number };
  };
  fuelLevel: number;
  fuel?: { level: number };
  performance?: {
    deltaTime: number;
    efficiency: number;
    lapNumber?: number;
    deltaToLap?: number;
  };
  level?: number;
};

/**
 * Server-side request/response types
 */

export interface CalculateStrategyRequest {
  input: StrategyInput;
}

export interface CalculateStrategyResponse {
  success: boolean;
  data?: StrategyOutput;
  error?: string;
  calculationTimeMs?: number;
}

export interface SaveStrategyRequest {
  sessionId: string;
  strategyInput: StrategyInput;
  strategyOutput: StrategyOutput;
  activeScenario: 'best' | 'likely' | 'worst';
}

export interface SaveStrategyResponse {
  success: boolean;
  strategyId?: string;
  error?: string;
}

export interface LogAdjustmentRequest {
  strategyId: string;
  adjustment: StrategyAdjustment;
}

export interface LogAdjustmentResponse {
  success: boolean;
  adjustmentId?: string;
  error?: string;
}

/**
 * Internal simulation types
 */

export interface SimulationContext {
  fuelPerLap: number;
  tireWearPerLap: number;
  pitDuration: number;
  safetyMargin: number;
  raceLength: number; // total laps
}

export interface PitSequenceSolution {
  stops: PitStop[];
  totalPitTime: number;
  totalTime: number;
  estimatedFinishFuel: number;
  score: number; // lower is better
}
