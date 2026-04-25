import React, { useEffect, useState } from 'react';
import { View, Text, ScrollView, StyleSheet, SafeAreaView, ActivityIndicator, Pressable } from 'react-native';
import { StatusBar } from 'expo-status-bar';

interface BackendStatus {
  status: 'connecting' | 'connected' | 'error' | 'offline';
  message: string;
  timestamp: string;
}

/**
 * Minimal working app - tests backend connectivity
 */
export default function MinimalApp() {
  const [backendStatus, setBackendStatus] = useState<BackendStatus>({
    status: 'connecting',
    message: 'Attempting to connect to backend...',
    timestamp: new Date().toLocaleTimeString(),
  });

  const checkBackend = async () => {
    try {
      setBackendStatus({
        status: 'connecting',
        message: 'Checking backend...',
        timestamp: new Date().toLocaleTimeString(),
      });

      const response = await fetch('http://localhost:3000/health', {
        method: 'GET',
      } as any);

      if (response.ok) {
        setBackendStatus({
          status: 'connected',
          message: '✓ Backend API is connected!',
          timestamp: new Date().toLocaleTimeString(),
        });
      } else {
        setBackendStatus({
          status: 'error',
          message: `⚠ Backend returned status ${response.status}`,
          timestamp: new Date().toLocaleTimeString(),
        });
      }
    } catch (error: any) {
      setBackendStatus({
        status: 'offline',
        message: `✗ Cannot reach backend`,
        timestamp: new Date().toLocaleTimeString(),
      });
    }
  };

  useEffect(() => {
    checkBackend();
    const interval = setInterval(checkBackend, 5000);
    return () => clearInterval(interval);
  }, []);

  const getStatusColor = () => {
    switch (backendStatus.status) {
      case 'connected':
        return '#00FF00';
      case 'error':
        return '#FFFF00';
      case 'offline':
        return '#FF4444';
      default:
        return '#00D9FF';
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar barStyle="light-content" backgroundColor="#000000" />
      <ScrollView style={styles.container} contentContainerStyle={styles.content}>
        {/* Header */}
        <View style={styles.header}>
          <Text style={styles.title}>🏁 PITWALL</Text>
          <Text style={styles.subtitle}>Simracing Telemetry Hub</Text>
        </View>

        {/* Frontend Status */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>✓ Frontend is Working!</Text>
          <Text style={styles.text}>React app rendering successfully</Text>
        </View>

        {/* Backend Status */}
        <View style={[styles.section, { borderLeftColor: getStatusColor() }]}>
          <Text style={styles.sectionTitle}>Backend Connection</Text>
          <View style={styles.statusContainer}>
            {backendStatus.status === 'connecting' && (
              <ActivityIndicator size="small" color="#00D9FF" style={{ marginRight: 8 }} />
            )}
            <Text style={[styles.statusText, { color: getStatusColor() }]}>
              {backendStatus.message}
            </Text>
          </View>
          <Text style={styles.timestamp}>{backendStatus.timestamp}</Text>
        </View>

        {/* API Endpoints */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Endpoints</Text>
          <Text style={styles.monospace}>Frontend: http://localhost:5173</Text>
          <Text style={styles.monospace}>Backend: http://localhost:3000</Text>
          <Text style={styles.monospace}>Database: localhost:5432</Text>
        </View>

        {/* Help */}
        {backendStatus.status === 'offline' && (
          <View style={[styles.section, { borderLeftColor: '#FF8B00' }]}>
            <Text style={styles.sectionTitle}>Backend Not Running</Text>
            <Text style={styles.text}>Start the backend with:</Text>
            <Text style={styles.code}>cd /workspaces/PitWall/backend</Text>
            <Text style={styles.code}>docker compose up --build</Text>
          </View>
        )}

        {backendStatus.status === 'connected' && (
          <View style={[styles.section, { borderLeftColor: '#00FF00' }]}>
            <Text style={styles.sectionTitle}>✓ Ready to Go!</Text>
            <Text style={styles.text}>Frontend and backend are connected.</Text>
            <Text style={styles.text}>Refresh to load the full application.</Text>
          </View>
        )}

        {/* Footer */}
        <Text style={styles.footer}>v1.0.0 • Professional Simracing Platform</Text>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#0A0E27',
  },
  container: {
    flex: 1,
    backgroundColor: '#0A0E27',
  },
  content: {
    padding: 20,
    flexGrow: 1,
  },
  header: {
    alignItems: 'center',
    marginBottom: 40,
    paddingBottom: 20,
    borderBottomWidth: 2,
    borderBottomColor: '#22334F',
  },
  title: {
    fontSize: 38,
    fontWeight: 'bold',
    color: '#FFFFFF',
    letterSpacing: 2,
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: '#00D9FF',
    letterSpacing: 1,
  },
  section: {
    backgroundColor: '#151b2f',
    borderRadius: 8,
    padding: 16,
    marginBottom: 20,
    borderLeftWidth: 3,
    borderLeftColor: '#FF8B00',
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#FFFFFF',
    marginBottom: 10,
  },
  text: {
    fontSize: 14,
    color: '#B0B0B0',
    marginBottom: 6,
  },
  monospace: {
    fontSize: 12,
    color: '#00FF00',
    fontFamily: 'monospace',
    marginBottom: 4,
  },
  code: {
    fontSize: 11,
    color: '#00FF00',
    fontFamily: 'monospace',
    backgroundColor: '#0A0E27',
    padding: 8,
    borderRadius: 4,
    marginBottom: 4,
  },
  statusContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  statusText: {
    fontSize: 14,
    fontWeight: '600',
  },
  timestamp: {
    fontSize: 12,
    color: '#808080',
    marginTop: 8,
    fontStyle: 'italic',
  },
  footer: {
    fontSize: 12,
    color: '#5A6B7D',
    textAlign: 'center',
    marginTop: 20,
  },
});
