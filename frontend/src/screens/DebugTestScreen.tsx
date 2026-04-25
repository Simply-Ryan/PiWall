import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

/**
 * Debug Test Screen - Verify app is rendering
 */
export const DebugTestScreen: React.FC = () => {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>🚀 PitWall Frontend is Running!</Text>
      <Text style={styles.subtitle}>If you see this, the app compiled successfully</Text>
      
      <View style={styles.section}>
        <Text style={styles.label}>Debugging Information:</Text>
        <Text style={styles.info}>• Node environment: {process.env.NODE_ENV}</Text>
        <Text style={styles.info}>• API URL: {process.env.EXPO_PUBLIC_API_URL}</Text>
        <Text style={styles.info}>• Environment: {process.env.EXPO_PUBLIC_ENV}</Text>
        <Text style={styles.info}>• Current time: {new Date().toLocaleTimeString()}</Text>
      </View>

      <View style={styles.section}>
        <Text style={styles.label}>If you see a blank white screen:</Text>
        <Text style={styles.info}>1. Open DevTools (F12)</Text>
        <Text style={styles.info}>2. Check Console tab for errors</Text>
        <Text style={styles.info}>3. Check if backend is running: http://localhost:3000/health</Text>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#000000',
    padding: 20,
    justifyContent: 'center',
  },
  title: {
    color: '#00FF00',
    fontSize: 28,
    fontWeight: 'bold',
    marginBottom: 10,
    textAlign: 'center',
  },
  subtitle: {
    color: '#00D9FF',
    fontSize: 16,
    textAlign: 'center',
    marginBottom: 30,
  },
  section: {
    backgroundColor: '#1a1f3a',
    padding: 15,
    borderRadius: 8,
    marginBottom: 15,
    borderLeftWidth: 3,
    borderLeftColor: '#FF8B00',
  },
  label: {
    color: '#FFFFFF',
    fontSize: 14,
    fontWeight: 'bold',
    marginBottom: 8,
  },
  info: {
    color: '#B0B0B0',
    fontSize: 13,
    marginBottom: 4,
    fontFamily: 'monospace',
  },
});
