/**
 * Fuel Strategy Redux Slice
 *
 * Manages global state for fuel calculations, predictions, and pit strategy
 * Tracks consumption history and notifies driver of fuel/pit events
 */

import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { FuelMetrics, RaceStrategy } from '../../services/FuelCalculator';

export interface FuelStrategyState {
  // Current metrics
  metrics: FuelMetrics | null;
  strategy: RaceStrategy | null;

  // Consumption tracking
  consumptionHistory: number[];
  currentLapConsumption: number;
  previousLapConsumption: number | null;

  // Fuel status
  status: 'safe' | 'warning' | 'danger' | 'unknown';
  previousStatus: 'safe' | 'warning' | 'danger' | 'unknown';

  // Alerts
  alerts: Array<{
    id: string;
    type: 'fuel_low' | 'pit_recommended' | 'consumption_spike' | 'trend_change';
    message: string;
    severity: 'info' | 'warning' | 'critical';
    timestamp: number;
    dismissed: boolean;
  }>;

  // Analysis
  trend: 'increasing' | 'stable' | 'decreasing' | null;
  bestConsumptionLap: number | null;
  worstConsumptionLap: number | null;

  // Session tracking
  sessionStartTime: number | null;
  lastUpdateTime: number;
  isCalculating: boolean;
  error: string | null;
}

const initialState: FuelStrategyState = {
  metrics: null,
  strategy: null,
  consumptionHistory: [],
  currentLapConsumption: 0,
  previousLapConsumption: null,
  status: 'unknown',
  previousStatus: 'unknown',
  alerts: [],
  trend: null,
  bestConsumptionLap: null,
  worstConsumptionLap: null,
  sessionStartTime: null,
  lastUpdateTime: Date.now(),
  isCalculating: false,
  error: null,
};

export const fuelStrategySlice = createSlice({
  name: 'fuelStrategy',
  initialState,
  reducers: {
    // Update main metrics and strategy
    updateMetrics: (state, action: PayloadAction<{ metrics: FuelMetrics; strategy: RaceStrategy }>) => {
      const { metrics, strategy } = action.payload;

      // Track status changes for alerts
      const wasStatusChange = state.status !== metrics.safetyStatus;
      if (wasStatusChange) {
        state.previousStatus = state.status;
        state.status = metrics.safetyStatus;
      }

      state.metrics = metrics;
      state.strategy = strategy;
      state.lastUpdateTime = Date.now();
      state.isCalculating = false;
      state.error = null;
    },

    // Track consumption for individual lap
    recordLapConsumption: (state, action: PayloadAction<number>) => {
      const consumption = action.payload;

      // Validate consumption (should be 0-20L for most cars)
      if (consumption >= 0 && consumption <= 20) {
        state.previousLapConsumption = state.currentLapConsumption;
        state.currentLapConsumption = consumption;
        state.consumptionHistory.push(consumption);

        // Keep only last 200 laps
        if (state.consumptionHistory.length > 200) {
          state.consumptionHistory.shift();
        }

        // Track best/worst consumption
        if (state.bestConsumptionLap === null || consumption < state.consumptionHistory[state.bestConsumptionLap]) {
          state.bestConsumptionLap = state.consumptionHistory.length - 1;
        }
        if (state.worstConsumptionLap === null || consumption > state.consumptionHistory[state.worstConsumptionLap]) {
          state.worstConsumptionLap = state.consumptionHistory.length - 1;
        }
      }
    },

    // Update consumption trend
    setTrend: (state, action: PayloadAction<'increasing' | 'stable' | 'decreasing' | null>) => {
      state.trend = action.payload;
    },

    // Add alert
    addAlert: (
      state,
      action: PayloadAction<{
        type: 'fuel_low' | 'pit_recommended' | 'consumption_spike' | 'trend_change';
        message: string;
        severity: 'info' | 'warning' | 'critical';
      }>
    ) => {
      const alert = {
        id: `alert-${Date.now()}-${Math.random()}`,
        ...action.payload,
        timestamp: Date.now(),
        dismissed: false,
      };

      // Prevent duplicate alerts in last 5 seconds
      const recentSimilar = state.alerts.find(
        (a) => a.type === alert.type && Date.now() - a.timestamp < 5000
      );

      if (!recentSimilar) {
        state.alerts.push(alert);

        // Keep only last 20 alerts
        if (state.alerts.length > 20) {
          state.alerts.shift();
        }
      }
    },

    // Dismiss alert
    dismissAlert: (state, action: PayloadAction<string>) => {
      const alert = state.alerts.find((a) => a.id === action.payload);
      if (alert) {
        alert.dismissed = true;
      }
    },

    // Clear all dismissed alerts
    clearDismissedAlerts: (state) => {
      state.alerts = state.alerts.filter((a) => !a.dismissed);
    },

    // Set calculating state
    setCalculating: (state, action: PayloadAction<boolean>) => {
      state.isCalculating = action.payload;
    },

    // Set error
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload;
    },

    // Start new session
    startSession: (state) => {
      state.sessionStartTime = Date.now();
      state.consumptionHistory = [];
      state.currentLapConsumption = 0;
      state.previousLapConsumption = null;
      state.status = 'unknown';
      state.previousStatus = 'unknown';
      state.alerts = [];
      state.trend = null;
      state.bestConsumptionLap = null;
      state.worstConsumptionLap = null;
      state.metrics = null;
      state.strategy = null;
      state.error = null;
    },

    // End session
    endSession: (state) => {
      state.sessionStartTime = null;
      // Keep history for review, but mark session as ended
    },

    // Reset to initial state
    reset: () => initialState,
  },
});

export const {
  updateMetrics,
  recordLapConsumption,
  setTrend,
  addAlert,
  dismissAlert,
  clearDismissedAlerts,
  setCalculating,
  setError,
  startSession,
  endSession,
  reset,
} = fuelStrategySlice.actions;

export default fuelStrategySlice.reducer;
