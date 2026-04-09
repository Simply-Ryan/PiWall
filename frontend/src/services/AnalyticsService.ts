/**
 * Analytics API Service
 *
 * Provides methods to fetch and analyze telemetry data from the backend
 * including fuel consumption analysis, lap statistics, and session analytics
 */

import axios, { AxiosInstance } from 'axios';

export interface FuelAnalysisResponse {
  sessionId: string;
  track: string;
  totalLaps: number;
  consumption: {
    average: number;
    min: number;
    max: number;
    consistency: number;
  };
  trend: {
    direction: 'increasing' | 'decreasing' | 'stable';
    percentChange: number;
  };
  fuelMetrics: {
    initialFuel: number;
    finalFuel: number;
    totalConsumed: number;
  };
  peaks: {
    bestConsumptionLap: number;
    worstConsumptionLap: number;
    bestConsumption: number;
    worstConsumption: number;
  };
  efficiency: number; // 0-100 score
  consumptionHistory: number[];
}

export interface SessionAnalyticsResponse {
  sessionId: string;
  bestLap: number;
  worstLap: number;
  avgLap: number;
  consistency: number;
  maxSpeed: number;
  avgSpeed: number;
  totalTime: number;
  personalBests: number;
}

class AnalyticsService {
  private api: AxiosInstance | null = null;

  /**
   * Initialize the service with API endpoint and auth token
   */
  setAPI(baseURL: string, token?: string) {
    this.api = axios.create({
      baseURL,
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      timeout: 30000,
    });
  }

  /**
   * Get fuel consumption analysis for a session
   */
  async getFuelAnalysis(sessionId: string): Promise<FuelAnalysisResponse> {
    if (!this.api) {
      throw new Error('Analytics service not initialized');
    }

    try {
      const response = await this.api.get<FuelAnalysisResponse>(`/analytics/fuel/${sessionId}`);
      return response.data;
    } catch (error) {
      console.error('Failed to fetch fuel analysis:', error);
      throw error;
    }
  }

  /**
   * Get general session analytics
   */
  async getSessionAnalytics(sessionId: string): Promise<SessionAnalyticsResponse> {
    if (!this.api) {
      throw new Error('Analytics service not initialized');
    }

    try {
      const response = await this.api.get<SessionAnalyticsResponse>(`/sessions/${sessionId}/analytics`);
      return response.data;
    } catch (error) {
      console.error('Failed to fetch session analytics:', error);
      throw error;
    }
  }

  /**
   * Export session telemetry as CSV
   */
  async exportTelemetryCSV(sessionId: string): Promise<Blob> {
    if (!this.api) {
      throw new Error('Analytics service not initialized');
    }

    try {
      const response = await this.api.get(`/sessions/${sessionId}/telemetry-export`, {
        responseType: 'blob',
      });
      return response.data;
    } catch (error) {
      console.error('Failed to export telemetry:', error);
      throw error;
    }
  }

  /**
   * Compare two laps
   */
  async compareLaps(sessionId: string, lap1: number, lap2: number) {
    if (!this.api) {
      throw new Error('Analytics service not initialized');
    }

    try {
      const response = await this.api.get(`/sessions/${sessionId}/lap-comparison`, {
        params: { lap1, lap2 },
      });
      return response.data;
    } catch (error) {
      console.error('Failed to compare laps:', error);
      throw error;
    }
  }

  /**
   * Get personal improvement trends
   */
  async getImprovementTrends() {
    if (!this.api) {
      throw new Error('Analytics service not initialized');
    }

    try {
      const response = await this.api.get('/analytics/trends');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch trends:', error);
      throw error;
    }
  }
}

// Export singleton instance
export const analyticsService = new AnalyticsService();
