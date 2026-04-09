/**
 * Fuel Strategy Screen
 *
 * Comprehensive fuel analysis and pit strategy planning interface
 * Displays consumption trends, predictions, and recommended pit windows
 */

import React, { useEffect } from 'react';
import {
  StyleSheet,
  View,
  Text,
  ScrollView,
  Pressable,
  Alert,
  Animated,
  Dimensions,
} from 'react-native';
import { useAppDispatch, useAppSelector } from '../redux/store';
import { useFuelStrategy } from '../hooks/useFuelStrategy';
import {
  updateMetrics,
  recordLapConsumption,
  setTrend,
  addAlert,
} from '../redux/slices/fuelStrategySlice';
import { getFuelStatusColor } from '../services/FuelCalculator';

const { width } = Dimensions.get('window');

interface FuelStrategyScreenProps {
  navigation?: any;
}

export const FuelStrategyScreen: React.FC<FuelStrategyScreenProps> = ({ navigation }) => {
  const dispatch = useAppDispatch();
  const fuelStrategy = useAppSelector((state) => state.fuelStrategy);
  const telemetry = useAppSelector((state) => state.telemetry);
  const session = useAppSelector((state) => state.session);

  // Use the fuel strategy hook for calculations
  const { metrics, strategy, trend, conservative, pitImpact, isCalculating, error } = useFuelStrategy({
    totalLaps: 15,
    safetyMargin: 1.5,
  });

  // Sync hook results to Redux
  useEffect(() => {
    if (metrics && strategy) {
      dispatch(updateMetrics({ metrics, strategy }));
      dispatch(setTrend(trend));
    }
  }, [metrics, strategy, trend, dispatch]);

  // Alert on status changes
  useEffect(() => {
    if (fuelStrategy.previousStatus !== fuelStrategy.status && fuelStrategy.status !== 'unknown') {
      const severityMap = {
        safe: 'info',
        warning: 'warning',
        danger: 'critical',
        unknown: 'info',
      };

      dispatch(
        addAlert({
          type: 'fuel_low',
          message: `Fuel status: ${fuelStrategy.status.toUpperCase()}`,
          severity: severityMap[fuelStrategy.status] as any,
        })
      );
    }
  }, [fuelStrategy.status, fuelStrategy.previousStatus, dispatch]);

  if (error) {
    return (
      <View style={styles.container}>
        <Text style={styles.errorText}>Error: {error}</Text>
      </View>
    );
  }

  if (!metrics) {
    return (
      <View style={styles.container}>
        <Text style={styles.placeholderText}>Waiting for telemetry data...</Text>
      </View>
    );
  }

  const statusColor = getFuelStatusColor(metrics.safetyStatus);

  return (
    <ScrollView style={styles.container} showsVerticalScrollIndicator={false}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.title}>FUEL STRATEGY</Text>
      </View>

      {/* Main Status Panel */}
      <View style={[styles.mainPanel, { borderColor: statusColor }]}>
        <View style={styles.statusRow}>
          <View style={styles.statusIndicator}>
            <View style={[styles.statusLight, { backgroundColor: statusColor }]} />
            <Text style={[styles.statusText, { color: statusColor }]}>
              {metrics.safetyStatus.toUpperCase()}
            </Text>
          </View>
          <View style={styles.willFinishIndicator}>
            <Text style={styles.willFinishLabel}>
              {metrics.willFinish ? '✓ CAN FINISH' : '✗ REFUEL NEEDED'}
            </Text>
            <Text style={[styles.willFinishValue, { color: metrics.willFinish ? '#00FF00' : '#FF4444' }]}>
              {metrics.predictedEndFuel.toFixed(2)}L
            </Text>
          </View>
        </View>
      </View>

      {/* Fuel Metrics Grid */}
      <View style={styles.metricsGrid}>
        <MetricCard label="CURRENT" value={`${metrics.currentLevel.toFixed(1)}L`} subtext="Fuel Level" />
        <MetricCard label="CAPACITY" value={`${metrics.capacity.toFixed(1)}L`} subtext="Tank" />
        <MetricCard
          label="AVG CONSUMPTION"
          value={`${metrics.avgConsumption.toFixed(3)}L`}
          subtext="Per Lap"
        />
        <MetricCard
          label="CONSISTENCY"
          value={`${metrics.consistency.toFixed(3)}`}
          subtext="Std Dev"
          color={metrics.consistency < 0.5 ? '#00FF00' : metrics.consistency < 1.0 ? '#FFFF00' : '#FF8800'}
        />
      </View>

      {/* Predictions */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>RACE PREDICTIONS</Text>
        <View style={styles.predictionCard}>
          <View style={styles.predictionRow}>
            <Text style={styles.predictionLabel}>Remaining Laps</Text>
            <Text style={styles.predictionValue}>{metrics.remainingLaps}</Text>
          </View>
          <View style={styles.predictionRow}>
            <Text style={styles.predictionLabel}>Currently Projected to End With</Text>
            <Text style={[styles.predictionValue, { color: statusColor }]}>
              {metrics.predictedEndFuel.toFixed(2)}L
            </Text>
          </View>
          <View style={styles.predictionRow}>
            <Text style={styles.predictionLabel}>Safety Margin</Text>
            <Text style={styles.predictionValue}>{metrics.safetyMargin.toFixed(1)}L</Text>
          </View>
          <View style={[styles.predictionRow, styles.borderTop]}>
            <Text style={styles.predictionLabel}>Margin Status</Text>
            <Text style={[styles.predictionValue, { color: statusColor }]}>
              {(metrics.predictedEndFuel - metrics.safetyMargin).toFixed(2)}L
            </Text>
          </View>
        </View>
      </View>

      {/* Pit Strategy */}
      {strategy && (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>PIT STRATEGY</Text>
          <View style={styles.strategyCard}>
            <View style={styles.strategyRow}>
              <Text style={styles.strategyLabel}>Total Pit Stops Needed</Text>
              <Text style={styles.strategyValue}>{strategy.stopCount}</Text>
            </View>
            <View style={styles.strategyRow}>
              <Text style={styles.strategyLabel}>Fuel Per Stop</Text>
              <Text style={styles.strategyValue}>{strategy.fuelPerStop}L</Text>
            </View>
            <View style={styles.strategyRow}>
              <Text style={styles.strategyLabel}>Laps Per Stop</Text>
              <Text style={styles.strategyValue}>{strategy.lapsPerStop}</Text>
            </View>

            {/* Pit Window */}
            <View style={[styles.strategyRow, styles.borderTop]}>
              <Text style={styles.strategyLabel}>Recommended Pit Window</Text>
            </View>
            <View style={styles.pitWindowContainer}>
              <View style={styles.pitWindowItem}>
                <Text style={styles.pitWindowLabel}>Earliest</Text>
                <Text style={styles.pitWindowValue}>L{strategy.pitWindow.earliest}</Text>
              </View>
              <View style={styles.pitWindowItem}>
                <Text style={styles.pitWindowLabel}>Recommended</Text>
                <Text style={[styles.pitWindowValue, { color: '#00FF00' }]}>
                  L{strategy.pitWindow.recommendedLap}
                </Text>
              </View>
              <View style={styles.pitWindowItem}>
                <Text style={styles.pitWindowLabel}>Latest</Text>
                <Text style={styles.pitWindowValue}>L{strategy.pitWindow.latest}</Text>
              </View>
            </View>
          </View>
        </View>
      )}

      {/* Consumption Trend */}
      {trend && (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>CONSUMPTION TREND</Text>
          <View style={styles.trendCard}>
            <View style={styles.trendIndicator}>
              <Text
                style={[
                  styles.trendArrow,
                  {
                    color:
                      trend === 'increasing'
                        ? '#FF4444'
                        : trend === 'decreasing'
                          ? '#00FF00'
                          : '#CCCCCC',
                  },
                ]}
              >
                {trend === 'increasing' ? '↑' : trend === 'decreasing' ? '↓' : '→'}
              </Text>
              <Text style={[styles.trendText, { marginLeft: 8 }]}>
                Consumption is {trend === 'increasing' ? 'INCREASING' : trend === 'decreasing' ? 'DECREASING' : 'STABLE'}
              </Text>
            </View>
            {conservative && (
              <View style={[styles.trendRow, styles.borderTop]}>
                <Text style={styles.trendLabel}>Conservative Estimate</Text>
                <Text style={styles.trendValue}>{conservative.minEndFuel.toFixed(2)}L</Text>
              </View>
            )}
          </View>
        </View>
      )}

      {/* Pit Impact Analysis */}
      {pitImpact && (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>PIT IMPACT ANALYSIS</Text>
          <View style={styles.impactCard}>
            <View style={styles.impactRow}>
              <Text style={styles.impactLabel}>Pit Stop Cost</Text>
              <Text style={styles.impactValue}>~{pitImpact.lapTimeLoss.toFixed(1)}s</Text>
            </View>
            <View style={[styles.impactRow, styles.borderTop]}>
              <Text style={[styles.impactLabel, { color: pitImpact.isFuelIssue ? '#FF4444' : '#00FF00' }]}>
                Fuel Critical
              </Text>
              <Text style={[styles.impactValue, { color: pitImpact.isFuelIssue ? '#FF4444' : '#00FF00' }]}>
                {pitImpact.isFuelIssue ? 'YES' : 'NO'}
              </Text>
            </View>
            <View style={[styles.impactRow, styles.borderTop]}>
              <Text style={styles.impactRecommendation}>{pitImpact.recommendation}</Text>
            </View>
          </View>
        </View>
      )}

      {/* Active Alerts */}
      {fuelStrategy.alerts.length > 0 && (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>ACTIVE ALERTS</Text>
          {fuelStrategy.alerts
            .filter((a) => !a.dismissed)
            .map((alert) => (
              <View
                key={alert.id}
                style={[
                  styles.alertBox,
                  {
                    borderLeftColor:
                      alert.severity === 'critical'
                        ? '#FF4444'
                        : alert.severity === 'warning'
                          ? '#FFFF00'
                          : '#00FF00',
                  },
                ]}
              >
                <Text style={styles.alertMessage}>{alert.message}</Text>
              </View>
            ))}
        </View>
      )}

      {/* Debug Info */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>DEBUG INFO</Text>
        <View style={styles.debugBox}>
          <DebugRow label="Laps Tracked" value={fuelStrategy.consumptionHistory.length} />
          <DebugRow label="Last Update" value={new Date(fuelStrategy.lastUpdateTime).toLocaleTimeString()} />
          <DebugRow label="Status" value={fuelStrategy.status} />
          {fuelStrategy.trend && <DebugRow label="Trend" value={fuelStrategy.trend} />}
        </View>
      </View>

      {/* Bottom Padding */}
      <View style={styles.spacer} />
    </ScrollView>
  );
};

/**
 * Metric Card Component
 */
const MetricCard: React.FC<{
  label: string;
  value: string;
  subtext: string;
  color?: string;
}> = ({ label, value, subtext, color = '#CCCCCC' }) => (
  <View style={styles.metricCard}>
    <Text style={styles.metricLabel}>{label}</Text>
    <Text style={[styles.metricValue, { color }]}>{value}</Text>
    <Text style={styles.metricSubtext}>{subtext}</Text>
  </View>
);

/**
 * Debug Row Component
 */
const DebugRow: React.FC<{ label: string; value: any }> = ({ label, value }) => (
  <View style={styles.debugRow}>
    <Text style={styles.debugLabel}>{label}:</Text>
    <Text style={styles.debugValue}>{String(value)}</Text>
  </View>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#0A0E27',
    paddingTop: 0,
  },

  header: {
    backgroundColor: '#111111',
    paddingVertical: 16,
    paddingHorizontal: 20,
    borderBottomWidth: 2,
    borderBottomColor: '#333333',
  },

  title: {
    color: '#FFFFFF',
    fontSize: 28,
    fontWeight: 'bold',
    letterSpacing: 2,
  },

  placeholderText: {
    color: '#666666',
    fontSize: 14,
    textAlign: 'center',
    marginTop: 40,
  },

  errorText: {
    color: '#FF4444',
    fontSize: 14,
    textAlign: 'center',
    marginTop: 40,
  },

  // Main Status Panel
  mainPanel: {
    marginHorizontal: 16,
    marginTop: 16,
    paddingHorizontal: 16,
    paddingVertical: 20,
    backgroundColor: '#111111',
    borderRadius: 8,
    borderWidth: 2,
    borderColor: '#444444',
  },

  statusRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },

  statusIndicator: {
    flexDirection: 'row',
    alignItems: 'center',
  },

  statusLight: {
    width: 12,
    height: 12,
    borderRadius: 6,
    marginRight: 12,
  },

  statusText: {
    fontSize: 18,
    fontWeight: 'bold',
    letterSpacing: 1,
  },

  willFinishIndicator: {
    alignItems: 'flex-end',
  },

  willFinishLabel: {
    color: '#888888',
    fontSize: 11,
    fontWeight: 'bold',
    letterSpacing: 0.5,
  },

  willFinishValue: {
    fontSize: 16,
    fontWeight: 'bold',
    marginTop: 4,
  },

  // Metrics Grid
  metricsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    paddingHorizontal: 16,
    marginTop: 16,
    justifyContent: 'space-between',
  },

  metricCard: {
    width: '48%',
    backgroundColor: '#111111',
    borderRadius: 8,
    padding: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#333333',
  },

  metricLabel: {
    color: '#888888',
    fontSize: 9,
    fontWeight: 'bold',
    letterSpacing: 1,
    marginBottom: 6,
  },

  metricValue: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 4,
  },

  metricSubtext: {
    color: '#666666',
    fontSize: 10,
  },

  // Sections
  section: {
    paddingHorizontal: 16,
    marginTop: 20,
  },

  sectionTitle: {
    color: '#888888',
    fontSize: 12,
    fontWeight: 'bold',
    letterSpacing: 1.5,
    marginBottom: 12,
  },

  // Prediction Card
  predictionCard: {
    backgroundColor: '#111111',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#333333',
    overflow: 'hidden',
  },

  predictionRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
  },

  predictionLabel: {
    color: '#888888',
    fontSize: 12,
    flex: 1,
  },

  predictionValue: {
    color: '#CCCCCC',
    fontSize: 13,
    fontWeight: 'bold',
  },

  borderTop: {
    borderTopWidth: 1,
    borderTopColor: '#333333',
  },

  // Strategy Card
  strategyCard: {
    backgroundColor: '#111111',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#333333',
    overflow: 'hidden',
  },

  strategyRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
  },

  strategyLabel: {
    color: '#888888',
    fontSize: 12,
  },

  strategyValue: {
    color: '#CCCCCC',
    fontSize: 14,
    fontWeight: 'bold',
  },

  // Pit Window
  pitWindowContainer: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderTopWidth: 1,
    borderTopColor: '#333333',
  },

  pitWindowItem: {
    alignItems: 'center',
  },

  pitWindowLabel: {
    color: '#888888',
    fontSize: 10,
    marginBottom: 4,
  },

  pitWindowValue: {
    color: '#CCCCCC',
    fontSize: 14,
    fontWeight: 'bold',
  },

  // Trend Card
  trendCard: {
    backgroundColor: '#111111',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#333333',
    overflow: 'hidden',
  },

  trendIndicator: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 16,
  },

  trendArrow: {
    fontSize: 24,
    fontWeight: 'bold',
  },

  trendText: {
    color: '#CCCCCC',
    fontSize: 14,
    fontWeight: 'bold',
  },

  trendRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
  },

  trendLabel: {
    color: '#888888',
    fontSize: 12,
  },

  trendValue: {
    color: '#CCCCCC',
    fontSize: 13,
    fontWeight: 'bold',
  },

  // Impact Card
  impactCard: {
    backgroundColor: '#111111',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#333333',
    overflow: 'hidden',
  },

  impactRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
  },

  impactLabel: {
    color: '#888888',
    fontSize: 12,
  },

  impactValue: {
    color: '#CCCCCC',
    fontSize: 14,
    fontWeight: 'bold',
  },

  impactRecommendation: {
    color: '#00FF00',
    fontSize: 13,
    fontStyle: 'italic',
    width: '100%',
  },

  // Alert Box
  alertBox: {
    backgroundColor: '#1a1a2e',
    borderLeftWidth: 4,
    borderRadius: 4,
    padding: 12,
    marginBottom: 8,
  },

  alertMessage: {
    color: '#FFFFFF',
    fontSize: 12,
  },

  // Debug Box
  debugBox: {
    backgroundColor: '#1a1a2e',
    borderRadius: 4,
    padding: 12,
    borderWidth: 1,
    borderColor: '#444444',
  },

  debugRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#333333',
  },

  debugLabel: {
    color: '#888888',
    fontSize: 11,
  },

  debugValue: {
    color: '#00FF00',
    fontSize: 11,
    fontFamily: 'monospace',
  },

  spacer: {
    height: 40,
  },
});
