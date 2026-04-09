/**
 * Dashboard Screen
 *
 * Screen wrapper for the Smart Dashboard HUD
 * Displays real-time telemetry with professional racing interface
 * Also provides quick access to strategy tools and settings
 */

import React from 'react';
import { View, StyleSheet, Pressable, Text } from 'react-native';
import { Dashboard } from '@components/Dashboard';
import { RootNavigationProp } from '../App';

/**
 * Dashboard Screen - Displays the Smart Dashboard HUD
 */
export const DashboardScreen: React.FC<{ navigation?: RootNavigationProp }> = ({ navigation }) => {
  return (
    <View style={styles.container}>
      <Dashboard />
      
      {/* Quick Access Buttons */}
      <View style={styles.quickAccessBar}>
        <Pressable
          style={styles.quickAccessButton}
          onPress={() => navigation?.navigate('FuelStrategy')}
        >
          <Text style={styles.quickAccessButtonText}>⛽ FUEL</Text>
        </Pressable>
        
        <Pressable
          style={styles.quickAccessButton}
          onPress={() => navigation?.navigate('VoiceSettings')}
        >
          <Text style={styles.quickAccessButtonText}>🎙️ VOICE</Text>
        </Pressable>
        
        <Pressable
          style={styles.quickAccessButton}
          onPress={() => navigation?.navigate('Home')}
        >
          <Text style={styles.quickAccessButtonText}>🏠 HOME</Text>
        </Pressable>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#000000',
  },
  
  quickAccessBar: {
    position: 'absolute',
    bottom: 16,
    left: 16,
    right: 16,
    flexDirection: 'row',
    justifyContent: 'space-around',
    backgroundColor: 'rgba(17, 17, 17, 0.95)',
    borderTopWidth: 2,
    borderTopColor: '#333333',
    borderRadius: 8,
    paddingVertical: 10,
    gap: 8,
  },
  
  quickAccessButton: {
    flex: 1,
    paddingHorizontal: 12,
    paddingVertical: 8,
    backgroundColor: '#1a1a2e',
    borderRadius: 6,
    borderWidth: 1,
    borderColor: '#444444',
    justifyContent: 'center',
    alignItems: 'center',
  },
  
  quickAccessButtonText: {
    color: '#00FF00',
    fontSize: 11,
    fontWeight: 'bold',
    letterSpacing: 0.5,
  },
});
