/**
 * Voice Settings Screen
 * 
 * Allows users to configure voice notifications including:
 * - Enable/disable voice feedback
 * - Volume and speech rate control
 * - Notification type preferences
 * - Voice selection
 */

import React, { useState } from 'react';
import {
  ScrollView,
  View,
  Text,
  StyleSheet,
  Switch,
  Slider,
  TouchableOpacity,
  Alert,
  SafeAreaView,
} from 'react-native';
import VoiceService from '../services/VoiceService';
import { useVoice } from '../hooks/useVoice';
import { VoiceNotificationType } from '../types/voice';

export const VoiceSettingsScreen: React.FC = () => {
  const { voiceSettings, updateSettings } = useVoice();
  const [testMessage, setTestMessage] = useState('');

  const handleToggleNotificationType = (type: VoiceNotificationType) => {
    const types = new Set(voiceSettings.notificationTypes);
    if (types.has(type)) {
      types.delete(type);
    } else {
      types.add(type);
    }
    updateSettings({ notificationTypes: types });
  };

  const handleTestSpeak = async () => {
    const message =
      testMessage || `Test message at ${voiceSettings.rate}x speed`;
    try {
      await VoiceService.speak({
        id: 'test',
        type: VoiceNotificationType.CUSTOM,
        message,
        timestamp: Date.now(),
        priority: 'low',
        spoken: false,
      });
      Alert.alert('Test', 'Speaking test message...');
    } catch (error) {
      Alert.alert('Error', 'Failed to speak test message');
    }
  };

  const notificationOptions = [
    { type: VoiceNotificationType.UPSHIFT, label: 'Upshift Callouts' },
    { type: VoiceNotificationType.DOWNSHIFT, label: 'Downshift Callouts' },
    { type: VoiceNotificationType.REDLINE_WARNING, label: 'Redline Warnings' },
    { type: VoiceNotificationType.LOW_FUEL, label: 'Low Fuel Warnings' },
    { type: VoiceNotificationType.FUEL_CRITICAL, label: 'Critical Fuel' },
    { type: VoiceNotificationType.TIRE_COLD, label: 'Cold Tire Warnings' },
    { type: VoiceNotificationType.TIRE_HOT, label: 'Hot Tire Warnings' },
    {
      type: VoiceNotificationType.NEW_PERSONAL_BEST,
      label: 'Personal Best Records',
    },
    { type: VoiceNotificationType.BRAKE_HARD, label: 'Hard Braking' },
  ];

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView style={styles.content}>
        {/* Header */}
        <View style={styles.section}>
          <Text style={styles.title}>🎙️ Voice Settings</Text>
        </View>

        {/* Master Enable/Disable */}
        <View style={styles.section}>
          <View style={styles.settingRow}>
            <Text style={styles.label}>Voice Notifications</Text>
            <Switch
              value={voiceSettings.enabled}
              onValueChange={(value) =>
                updateSettings({ enabled: value })
              }
              trackColor={{ false: '#767577', true: '#81C784' }}
              thumbColor={voiceSettings.enabled ? '#4CAF50' : '#f4f3f4'}
            />
          </View>
        </View>

        {/* Volume Control */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Volume</Text>
          <View style={styles.sliderContainer}>
            <Text style={styles.sliderLabel}>
              {Math.round(voiceSettings.volume * 100)}%
            </Text>
            <Slider
              style={styles.slider}
              minimumValue={0}
              maximumValue={1}
              step={0.1}
              value={voiceSettings.volume}
              onValueChange={(value) =>
                updateSettings({ volume: value })
              }
            />
          </View>
        </View>

        {/* Speech Rate Control */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Speech Rate</Text>
          <View style={styles.sliderContainer}>
            <Text style={styles.sliderLabel}>
              {voiceSettings.rate.toFixed(1)}x
            </Text>
            <Slider
              style={styles.slider}
              minimumValue={0.5}
              maximumValue={2}
              step={0.1}
              value={voiceSettings.rate}
              onValueChange={(value) =>
                updateSettings({ rate: value })
              }
            />
          </View>
        </View>

        {/* Pitch Control */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Pitch</Text>
          <View style={styles.sliderContainer}>
            <Text style={styles.sliderLabel}>
              {voiceSettings.pitch.toFixed(1)}x
            </Text>
            <Slider
              style={styles.slider}
              minimumValue={0.5}
              maximumValue={2}
              step={0.1}
              value={voiceSettings.pitch}
              onValueChange={(value) =>
                updateSettings({ pitch: value })
              }
            />
          </View>
        </View>

        {/* Throttling Control */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Callout Throttling</Text>
          <View style={styles.sliderContainer}>
            <Text style={styles.sliderLabel}>
              {voiceSettings.minTimeBetweenCallouts}ms
            </Text>
            <Slider
              style={styles.slider}
              minimumValue={200}
              maximumValue={2000}
              step={100}
              value={voiceSettings.minTimeBetweenCallouts}
              onValueChange={(value) =>
                updateSettings({ minTimeBetweenCallouts: value })
              }
            />
          </View>
          <Text style={styles.helperText}>
            Minimum time between similar callouts
          </Text>
        </View>

        {/* Muting Options */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Muting Options</Text>
          <View style={styles.settingRow}>
            <Text style={styles.label}>Mute during hard braking</Text>
            <Switch
              value={voiceSettings.muteWhenBraking}
              onValueChange={(value) =>
                updateSettings({ muteWhenBraking: value })
              }
            />
          </View>
          <View style={styles.settingRow}>
            <Text style={styles.label}>Mute at high throttle</Text>
            <Switch
              value={voiceSettings.muteWhenThrottling}
              onValueChange={(value) =>
                updateSettings({ muteWhenThrottling: value })
              }
            />
          </View>
        </View>

        {/* Notification Preferences */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Notification Types</Text>
          {notificationOptions.map((option) => (
            <TouchableOpacity
              key={option.type}
              style={styles.checkboxRow}
              onPress={() =>
                handleToggleNotificationType(option.type)
              }
            >
              <View
                style={[
                  styles.checkbox,
                  voiceSettings.notificationTypes.has(option.type) &&
                    styles.checkboxChecked,
                ]}
              >
                {voiceSettings.notificationTypes.has(option.type) && (
                  <Text style={styles.checkmark}>✓</Text>
                )}
              </View>
              <Text style={styles.checkboxLabel}>{option.label}</Text>
            </TouchableOpacity>
          ))}
        </View>

        {/* Test Speaker */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Test Voice</Text>
          <TouchableOpacity
            style={styles.testButton}
            onPress={handleTestSpeak}
          >
            <Text style={styles.testButtonText}>🔊 Test Speaker</Text>
          </TouchableOpacity>
        </View>

        {/* Info */}
        <View style={styles.section}>
          <Text style={styles.info}>
            Voice callouts are automatically triggered during racing based on
            real-time telemetry data. Customize above to personalize your
            experience.
          </Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#1a1a1a',
  },
  content: {
    flex: 1,
    padding: 16,
  },
  section: {
    marginBottom: 24,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 8,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#4CAF50',
    marginBottom: 12,
  },
  settingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#333',
  },
  label: {
    fontSize: 14,
    color: '#fff',
  },
  sliderContainer: {
    paddingVertical: 8,
  },
  sliderLabel: {
    fontSize: 12,
    color: '#999',
    marginBottom: 8,
    textAlign: 'right',
  },
  slider: {
    width: '100%',
    height: 40,
  },
  helperText: {
    fontSize: 12,
    color: '#999',
    marginTop: 4,
  },
  checkboxRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#333',
  },
  checkbox: {
    width: 24,
    height: 24,
    borderWidth: 2,
    borderColor: '#4CAF50',
    borderRadius: 4,
    marginRight: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  checkboxChecked: {
    backgroundColor: '#4CAF50',
  },
  checkmark: {
    color: '#1a1a1a',
    fontSize: 14,
    fontWeight: 'bold',
  },
  checkboxLabel: {
    fontSize: 14,
    color: '#fff',
    flex: 1,
  },
  testButton: {
    backgroundColor: '#4CAF50',
    paddingVertical: 12,
    paddingHorizontal: 16,
    borderRadius: 8,
    alignItems: 'center',
  },
  testButtonText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#1a1a1a',
  },
  info: {
    fontSize: 12,
    color: '#999',
    fontStyle: 'italic',
    lineHeight: 18,
  },
});
