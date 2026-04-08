import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface SessionState {
  id: string | null;
  name: string | null;
  game: string | null;
  startTime: number | null;
  isRecording: boolean;
}

const initialState: SessionState = {
  id: null,
  name: null,
  game: null,
  startTime: null,
  isRecording: false,
};

export const sessionSlice = createSlice({
  name: 'session',
  initialState,
  reducers: {
    startSession: (
      state,
      action: PayloadAction<{ id: string; name: string; game: string }>
    ) => {
      state.id = action.payload.id;
      state.name = action.payload.name;
      state.game = action.payload.game;
      state.startTime = Date.now();
      state.isRecording = true;
    },
    stopSession: (state) => {
      state.isRecording = false;
    },
    endSession: (state) => {
      state.id = null;
      state.name = null;
      state.game = null;
      state.startTime = null;
      state.isRecording = false;
    },
  },
});

export const { startSession, stopSession, endSession } = sessionSlice.actions;

export default sessionSlice.reducer;
