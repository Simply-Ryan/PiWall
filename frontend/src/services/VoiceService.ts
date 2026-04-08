/**
 * Voice Service
 * 
 * Handles Text-to-Speech (TTS) functionality using Expo's Speech API
 * Manages voice settings, speaking queue, and callout logic
 */

import * as Speech from 'expo-speech';
import { VoiceSettings, VoiceNotification, VoiceNotificationType } from '../types/voice';

class VoiceService {
  private static instance: VoiceService;
  private settings: VoiceSettings;
  private isInitialized = false;
  private queue: VoiceNotification[] = [];
  private isSpeaking = false;
  private lastSpokenTime = 0;
  private availableVoices: string[] = [];

  private constructor() {
    this.settings = this.getDefaultSettings();
  }

  /**
   * Get singleton instance
   */
  static getInstance(): VoiceService {
    if (!VoiceService.instance) {
      VoiceService.instance = new VoiceService();
    }
    return VoiceService.instance;
  }

  /**
   * Initialize the voice service
   */
  async initialize(): Promise<void> {
    if (this.isInitialized) return;

    try {
      // Get available voices
      this.availableVoices = await Speech.getAvailableVoicesAsync();
      console.log('Available voices:', this.availableVoices);

      this.isInitialized = true;
    } catch (error) {
      console.error('Error initializing voice service:', error);
      throw error;
    }
  }

  /**
   * Speak a notification
   */
  async speak(notification: VoiceNotification): Promise<void> {
    if (!this.isInitialized || !this.settings.enabled) {
      return;
    }

    // Check if this notification type is enabled
    if (!this.settings.notificationTypes.has(notification.type)) {
      return;
    }

    // Check throttling - minimum time between callouts
    const now = Date.now();
    if (now - this.lastSpokenTime < this.settings.minTimeBetweenCallouts) {
      console.log('Callout throttled - too soon after last callout');
      return;
    }

    try {
      await Speech.speak(notification.message, {
        language: 'en-US',
        pitch: this.settings.pitch,
        rate: this.settings.rate,
        volume: this.settings.volume,
        onDone: () => {
          this.isSpeaking = false;
          this.processQueue();
        },
        onError: (error) => {
          console.error('Speech error:', error);
          this.isSpeaking = false;
          this.processQueue();
        },
      });

      this.isSpeaking = true;
      this.lastSpokenTime = now;
      notification.spoken = true;
    } catch (error) {
      console.error('Error speaking notification:', error);
    }
  }

  /**
   * Queue a notification to be spoken
   */
  async queueNotification(notification: VoiceNotification): Promise<void> {
    this.queue.push(notification);

    if (!this.isSpeaking) {
      this.processQueue();
    }
  }

  /**
   * Process notification queue
   */
  private async processQueue(): Promise<void> {
    if (this.queue.length === 0 || this.isSpeaking) {
      return;
    }

    const notification = this.queue.shift();
    if (notification) {
      await this.speak(notification);
    }
  }

  /**
   * Stop speaking immediately
   */
  async stop(): Promise<void> {
    try {
      await Speech.stop();
      this.isSpeaking = false;
    } catch (error) {
      console.error('Error stopping speech:', error);
    }
  }

  /**
   * Update voice settings
   */
  updateSettings(settings: Partial<VoiceSettings>): void {
    this.settings = {
      ...this.settings,
      ...settings,
      // Ensure notificationTypes array is preserved if not provided
      notificationTypes:
        settings.notificationTypes || this.settings.notificationTypes,
    };
  }

  /**
   * Get current settings
   */
  getSettings(): VoiceSettings {
    return { ...this.settings };
  }

  /**
   * Get available voices
   */
  getAvailableVoices(): string[] {
    return [...this.availableVoices];
  }

  /**
   * Enable/disable specific notification type
   */
  toggleNotificationType(type: VoiceNotificationType): void {
    const types = new Set(this.settings.notificationTypes);
    if (types.has(type)) {
      types.delete(type);
    } else {
      types.add(type);
    }
    this.settings.notificationTypes = types;
  }

  /**
   * Check is speaking
   */
  getIsSpeaking(): boolean {
    return this.isSpeaking;
  }

  /**
   * Get queue size
   */
  getQueueSize(): number {
    return this.queue.length;
  }

  /**
   * Clear queue
   */
  clearQueue(): void {
    this.queue = [];
  }

  /**
   * Get default settings
   */
  private getDefaultSettings(): VoiceSettings {
    return {
      enabled: true,
      volume: 0.8,
      rate: 1.0,
      pitch: 1.0,
      voiceId: 'default',
      notificationTypes: new Set([
        VoiceNotificationType.UPSHIFT,
        VoiceNotificationType.DOWNSHIFT,
        VoiceNotificationType.REDLINE_WARNING,
        VoiceNotificationType.LOW_FUEL,
        VoiceNotificationType.FUEL_CRITICAL,
        VoiceNotificationType.NEW_PERSONAL_BEST,
      ]),
      muteWhenBraking: false,
      muteWhenThrottling: false,
      minTimeBetweenCallouts: 500,
    };
  }
}

export default VoiceService.getInstance();
