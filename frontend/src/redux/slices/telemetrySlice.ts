import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface TelemetryState {
  data: {
    timestamp: number;
    vehicle: {
      speed: number;
      rpm: number;
      gear: number;
      throttle: number;
      brake: number;
    };
    tires: {
      [key: string]: {
        temperature: [number, number, number]; // inner, middle, outer
        wear: number;
      };
    };
    fuel: {
      level: number;
      consumed: number;
    };
    performance: {
      deltaToLap: number;
      lapNumber: number;
    };
  } | null;
  isConnected: boolean;
  error: string | null;
}

const initialState: TelemetryState = {
  data: null,
  isConnected: false,
  error: null,
};

export const telemetrySlice = createSlice({
  name: 'telemetry',
  initialState,
  reducers: {
    updateTelemetry: (state, action: PayloadAction<TelemetryState['data']>) => {
      state.data = action.payload;
      state.error = null;
    },
    setConnected: (state, action: PayloadAction<boolean>) => {
      state.isConnected = action.payload;
    },
    setError: (state, action: PayloadAction<string>) => {
      state.error = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
  },
});

export const {
  updateTelemetry,
  setConnected,
  setError,
  clearError,
} = telemetrySlice.actions;

export default telemetrySlice.reducer;
