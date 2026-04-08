import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface Notification {
  id: string;
  type: 'info' | 'warning' | 'error' | 'success';
  message: string;
  duration?: number;
}

export interface UIState {
  notifications: Notification[];
  isLoading: boolean;
  currentScreen: string;
  voiceSpotterEnabled: boolean;
}

const initialState: UIState = {
  notifications: [],
  isLoading: false,
  currentScreen: 'Home',
  voiceSpotterEnabled: true,
};

export const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    addNotification: (state, action: PayloadAction<Notification>) => {
      state.notifications.push(action.payload);
    },
    removeNotification: (state, action: PayloadAction<string>) => {
      state.notifications = state.notifications.filter((n) => n.id !== action.payload);
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    setCurrentScreen: (state, action: PayloadAction<string>) => {
      state.currentScreen = action.payload;
    },
    toggleVoiceSpotter: (state) => {
      state.voiceSpotterEnabled = !state.voiceSpotterEnabled;
    },
  },
});

export const {
  addNotification,
  removeNotification,
  setLoading,
  setCurrentScreen,
  toggleVoiceSpotter,
} = uiSlice.actions;

export default uiSlice.reducer;
