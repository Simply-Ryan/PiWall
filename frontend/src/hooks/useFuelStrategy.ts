/**
 * useFuelStrategy Hook
 *
 * Manages fuel strategy calculations in real-time during a race session
 * Integrates with Redux telemetry state and provides fuel predictions
 */

import { useEffect, useCallback, useRef, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
  calculateFuelMetrics,
  calculateRaceStrategy,
  analyzeTrend,
  conservativeAnalysis,
  calculatePitImpact,
  FuelMetrics,
  RaceStrategy,
} from '../services/FuelCalculator';

interface RaceConfig {
  totalLaps?: number;
  trackName?: string;
  simulatorType?: string;
  safetyMargin?: number; // liters
}

interface FuelStrategyState {
  metrics: FuelMetrics | null;
  strategy: RaceStrategy | null;
  trend: 'increasing' | 'stable' | 'decreasing' | null;
  conservative: {
    minEndFuel: number;
    maxConsumption: number;
    recommendedStop: boolean;
  } | null;
  pitImpact: {
    lapTimeLoss: number;
    isFuelIssue: boolean;
    recommendation: string;
  } | null;
  isCalculating: boolean;
  error: string | null;
}

export function useFuelStrategy(raceConfig: RaceConfig = {}) {
  const dispatch = useDispatch();

  // Get telemetry data from Redux (you'll need to adjust the selector paths based on your Redux structure)
  const telemetry = useSelector((state: any) => state.telemetry?.current || {});
  const sessionData = useSelector((state: any) => state.session?.current || {});

  // Local state tracking
  const consumptionHistoryRef = useRef<number[]>([]);
  const lastFuelLevelRef = useRef<number | null>(null);
  const lapCountRef = useRef<number>(0);

  const [state, setState] = useState<FuelStrategyState>({
    metrics: null,
    strategy: null,
    trend: null,
    conservative: null,
    pitImpact: null,
    isCalculating: false,
    error: null,
  });

  /**
   * Update consumption history with new fuel data
   */
  const updateConsumption = useCallback(() => {
    if (telemetry.fuel !== undefined && lastFuelLevelRef.current !== null) {
      const consumption = Math.max(0, lastFuelLevelRef.current - telemetry.fuel);

      // Only record if consumption is reasonable (0-20 liters per lap)
      if (consumption >= 0 && consumption <= 20) {
        consumptionHistoryRef.current.push(consumption);

        // Keep only last 100 laps for memory efficiency
        if (consumptionHistoryRef.current.length > 100) {
          consumptionHistoryRef.current.shift();
        }
      }

      lastFuelLevelRef.current = telemetry.fuel;
    }
  }, [telemetry.fuel]);

  /**
   * Calculate all fuel metrics and predictions
   */
  const calculateMetrics = useCallback(() => {
    const currentFuel = telemetry.fuel ?? 0;
    const capacity = raceConfig?.fuelCapacity ?? 100;
    const safetyMargin = raceConfig.safetyMargin ?? 1.5;

    // Get remaining laps
    const totalLaps = raceConfig.totalLaps || sessionData.totalLaps || 15;
    const currentLap = sessionData.currentLap || lapCountRef.current || 1;
    const remainingLaps = Math.max(1, totalLaps - currentLap);

    setState((prev) => ({ ...prev, isCalculating: true }));

    try {
      // Calculate main metrics
      const metrics = calculateFuelMetrics(
        currentFuel,
        capacity,
        consumptionHistoryRef.current,
        remainingLaps,
        safetyMargin
      );

      // Calculate race strategy
      const strategy = calculateRaceStrategy(
        currentLap,
        totalLaps,
        currentFuel,
        capacity,
        metrics.avgConsumption
      );

      // Analyze trend
      const trend = consumptionHistoryRef.current.length > 0 ? analyzeTrend(consumptionHistoryRef.current) : null;

      // Conservative analysis
      const conservative =
        consumptionHistoryRef.current.length > 0
          ? conservativeAnalysis(currentFuel, consumptionHistoryRef.current, remainingLaps)
          : null;

      // Pit impact analysis
      const pitImpact =
        metrics.avgConsumption > 0
          ? calculatePitImpact(currentLap, totalLaps, currentFuel, metrics.avgConsumption)
          : null;

      setState({
        metrics,
        strategy,
        trend,
        conservative,
        pitImpact,
        isCalculating: false,
        error: null,
      });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error in fuel calculations';
      setState((prev) => ({
        ...prev,
        isCalculating: false,
        error: errorMessage,
      }));
    }
  }, [telemetry.fuel, raceConfig, sessionData]);

  /**
   * Handle new lap detection
   */
  const handleLapChange = useCallback(() => {
    const newLap = sessionData.currentLap || 1;
    if (newLap !== lapCountRef.current) {
      updateConsumption();
      lapCountRef.current = newLap;
    }
  }, [sessionData.currentLap, updateConsumption]);

  /**
   * Clear consumption history (for new session)
   */
  const resetHistory = useCallback(() => {
    consumptionHistoryRef.current = [];
    lastFuelLevelRef.current = null;
    lapCountRef.current = 0;
    setState({
      metrics: null,
      strategy: null,
      trend: null,
      conservative: null,
      pitImpact: null,
      isCalculating: false,
      error: null,
    });
  }, []);

  /**
   * Update fuel level reference (called frequently)
   */
  const updateFuelReference = useCallback(() => {
    if (telemetry.fuel !== undefined && lastFuelLevelRef.current === null) {
      lastFuelLevelRef.current = telemetry.fuel;
    }
  }, [telemetry.fuel]);

  // Main effect - handle telemetry updates
  useEffect(() => {
    updateFuelReference();
    handleLapChange();
    calculateMetrics();
  }, [telemetry.fuel, sessionData.currentLap, calculateMetrics, updateFuelReference, handleLapChange]);

  // Reset on new session
  useEffect(() => {
    if (sessionData.isNewSession) {
      resetHistory();
    }
  }, [sessionData.isNewSession, resetHistory]);

  return {
    ...state,
    resetHistory,
    consumptionHistoryLength: consumptionHistoryRef.current.length,
  };
}
