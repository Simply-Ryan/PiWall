// Frontend test setup file
import '@testing-library/jest-native/extend-expect';

// Mock async storage
jest.mock('@react-native-async-storage/async-storage', () => ({
  setItem: jest.fn(async () => null),
  getItem: jest.fn(async () => null),
  removeItem: jest.fn(async () => null),
  clear: jest.fn(async () => null),
}));

// Mock speech API
jest.mock('expo-speech', () => ({
  speak: jest.fn(),
  stop: jest.fn(),
}));

// Setup environment
beforeAll(() => {
  process.env.EXPO_PUBLIC_API_URL = 'http://localhost:3001';
  process.env.EXPO_PUBLIC_WS_URL = 'ws://localhost:43200';
  process.env.EXPO_PUBLIC_APP_ENV = 'test';
});
