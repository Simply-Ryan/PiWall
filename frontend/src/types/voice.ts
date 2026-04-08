/**
 * Voice Notifications Types
 * 
 * Defines types for TTS-powered real-time callouts during racing sessions
 */

/** Types of voice notifications */
export enum VoiceNotificationType {
  // Gear and RPM
  THROTTLE_UP = 'THROTTLE_UP',
  BRAKE_HARD = 'BRAKE_HARD',
  UPSHIFT = 'UPSHIFT',
  DOWNSHIFT = 'DOWNSHIFT',
  REDLINE_WARNING = 'REDLINE_WARNING',

  // Tire notifications
  TIRE_COLD = 'TIRE_COLD',
  TIRE_HOT = 'TIRE_HOT',
  TIRE_WEAR_HIGH = 'TIRE_WEAR_HIGH',

  // Fuel notifications
  LOW_FUEL = 'LOW_FUEL',
  FUEL_CRITICAL = 'FUEL_CRITICAL',
  FUEL_TO_END = 'FUEL_TO_END',

  // Session notifications
  NEW_PERSONAL_BEST = 'NEW_PERSONAL_BEST',
  SLOWER_THAN_BEST = 'SLOWER_THAN_BEST',
  LEADER_PIT = 'LEADER_PIT',
  POSITION_CHANGE = 'POSITION_CHANGE',

  // Track announcements
  TRACK_LIMITS = 'TRACK_LIMITS',
  CORNER_WARNING = 'CORNER_WARNING',
  WEATHER_CHANGE = 'WEATHER_CHANGE',

  // Custom notifications
  CUSTOM = 'CUSTOM',
}

/** Voice notification configuration */
export interface VoiceNotification {
  id: string;
  type: VoiceNotificationType;
  message: string;
  timestamp: number;
  priority: 'low' | 'medium' | 'high' | 'critical';
  spoken: boolean;
  // Optional parameters for dynamic callouts
  context?: {
    speed?: number;
    rpm?: number;
    gear?: number;
    fuelLevel?: number;
    tireTemp?: number;
    lapTime?: number;
    [key: string]: unknown;
  };
}

/** TTS voice settings */
export interface VoiceSettings {
  enabled: boolean;
  volume: number; // 0-1
  rate: number; // 0.5-2.0 (1.0 = normal speed)
  pitch: number; // 0.5-2.0
  voiceId: string; // e.g., 'Daniel', 'Samantha', etc.
  
  // Notification preferences
  notificationTypes: Set<VoiceNotificationType>;
  muteWhenBraking: boolean; // Don't speak while heavy braking
  muteWhenThrottling: boolean; // Don't speak while at high throttle
  minTimeBetweenCallouts: number; // milliseconds (e.g., 500)
}

/** Voice notification event for Redux dispatch */
export interface VoiceNotificationEvent {
  type: VoiceNotificationType;
  message: string;
  priority?: 'low' | 'medium' | 'high' | 'critical';
  context?: Record<string, unknown>;
}

/** TTS service state */
export interface TextToSpeechState {
  isAvailable: boolean;
  isInitialized: boolean;
  isSpeaking: boolean;
  voicesAvailable: string[];
  currentVoice: string;
  error: string | null;
}
