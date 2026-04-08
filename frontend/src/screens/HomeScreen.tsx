import React, { useEffect } from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { useAppSelector, useAppDispatch } from '@redux/store';
import { setCurrentScreen } from '@redux/slices/uiSlice';

/**
 * Home Screen Component
 * 
 * TODO: Replace with actual dashboard HUD
 * Currently shows placeholder for development
 */
export const HomeScreen: React.FC = () => {
  const dispatch = useAppDispatch();
  const telemetry = useAppSelector((state) => state.telemetry);
  const session = useAppSelector((state) => state.session);

  useEffect(() => {
    dispatch(setCurrentScreen('Home'));
  }, [dispatch]);

  return (
    <View style={styles.container}>
      <Text style={styles.title}>🏁 PitWall</Text>
      <Text style={styles.subtitle}>Simracing Telemetry Hub</Text>

      {/* Status */}
      <View style={styles.statusSection}>
        <Text style={styles.label}>Connection Status</Text>
        <Text style={[styles.status, telemetry.isConnected ? styles.connected : styles.disconnected]}>
          {telemetry.isConnected ? '🟢 Connected' : '🔴 Disconnected'}
        </Text>
      </View>

      {/* Session Info */}
      <View style={styles.statusSection}>
        <Text style={styles.label}>Session</Text>
        {session.isRecording ? (
          <Text style={styles.recording}>🔴 RECORDING: {session.name || 'Unnamed'}</Text>
        ) : (
          <Text style={styles.idle}>⚪ Idle</Text>
        )}
      </View>

      {/* Telemetry Data */}
      {telemetry.data && (
        <View style={styles.telemSection}>
          <Text style={styles.label}>Live Telemetry</Text>
          <Text style={styles.telemData}>Speed: {telemetry.data.vehicle.speed} km/h</Text>
          <Text style={styles.telemData}>Gear: {telemetry.data.vehicle.gear}</Text>
          <Text style={styles.telemData}>RPM: {telemetry.data.vehicle.rpm}</Text>
        </View>
      )}

      {/* Development Notes */}
      <View style={styles.footer}>
        <Text style={styles.note}>v0.1.0 - Phase 1: Foundation</Text>
        <Text style={styles.note}>Phase 2 coming soon: Live Dashboard</Text>
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
    fontSize: 32,
    fontWeight: 'bold',
    color: '#FF6B6B',
    textAlign: 'center',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: '#888888',
    textAlign: 'center',
    marginBottom: 40,
  },
  statusSection: {
    marginBottom: 30,
    borderBottomWidth: 1,
    borderBottomColor: '#222222',
    paddingBottom: 15,
  },
  label: {
    fontSize: 12,
    color: '#666666',
    textTransform: 'uppercase',
    marginBottom: 8,
  },
  status: {
    fontSize: 16,
    fontWeight: '600',
  },
  connected: {
    color: '#00FF00',
  },
  disconnected: {
    color: '#FF0000',
  },
  recording: {
    fontSize: 16,
    fontWeight: '600',
    color: '#FF0000',
  },
  idle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#888888',
  },
  telemSection: {
    marginBottom: 30,
    backgroundColor: '#111111',
    padding: 15,
    borderRadius: 8,
  },
  telemData: {
    fontSize: 14,
    color: '#CCCCCC',
    marginTop: 8,
    fontFamily: 'Courier New',
  },
  footer: {
    marginTop: 50,
    borderTopWidth: 1,
    borderTopColor: '#222222',
    paddingTop: 15,
  },
  note: {
    fontSize: 12,
    color: '#666666',
    textAlign: 'center',
    marginBottom: 4,
  },
});
