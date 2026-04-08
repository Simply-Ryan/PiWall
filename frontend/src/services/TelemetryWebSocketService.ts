/**
 * WebSocket service for connecting to Telemetry Bridge
 * Handles real-time telemetry data streaming
 */

interface WebSocketConfig {
  url: string;
  reconnectAttempts: number;
  reconnectDelay: number; // milliseconds
  heartbeatInterval: number; // milliseconds
}

export interface TelemetryMessage {
  type: 'telemetry' | 'status' | 'error' | 'connected' | 'disconnected';
  data?: unknown;
  timestamp: number;
}

export class TelemetryWebSocketService {
  private ws: WebSocket | null = null;
  private config: WebSocketConfig;
  private reconnectAttempts = 0;
  private heartbeatTimer: number | null = null;
  private listeners: Map<string, ((message: TelemetryMessage) => void)[]>;

  constructor(config: Partial<WebSocketConfig> = {}) {
    this.config = {
      url: config.url || 'ws://localhost:43200',
      reconnectAttempts: config.reconnectAttempts || 5,
      reconnectDelay: config.reconnectDelay || 3000,
      heartbeatInterval: config.heartbeatInterval || 30000,
    };
    this.listeners = new Map();
  }

  /**
   * Connects to the telemetry WebSocket server
   */
  public connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        this.ws = new WebSocket(this.config.url);

        this.ws.onopen = () => {
          console.log('🟢 Connected to Telemetry Bridge');
          this.reconnectAttempts = 0;
          this.startHeartbeat();
          this.emit('connected', { type: 'connected', data: {}, timestamp: Date.now() });
          resolve();
        };

        this.ws.onmessage = (event) => {
          try {
            const message: TelemetryMessage = JSON.parse(event.data);
            this.emit('telemetry', message);
          } catch (error) {
            console.error('Failed to parse telemetry message:', error);
          }
        };

        this.ws.onerror = (event) => {
          console.error('❌ WebSocket error:', event);
          reject(new Error('WebSocket connection failed'));
        };

        this.ws.onclose = () => {
          console.log('⚫ Disconnected from Telemetry Bridge');
          this.stopHeartbeat();
          this.emit('disconnected', { type: 'disconnected', data: {}, timestamp: Date.now() });
          this.attemptReconnect();
        };
      } catch (error) {
        reject(error);
      }
    });
  }

  /**
   * Disconnects from the telemetry server
   */
  public disconnect(): void {
    this.stopHeartbeat();
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
  }

  /**
   * Sends a message to the server
   */
  public send(message: unknown): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message));
    } else {
      console.warn('WebSocket not connected');
    }
  }

  /**
   * Subscribes to telemetry messages
   */
  public subscribe(
    event: string,
    callback: (message: TelemetryMessage) => void
  ): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }

    const callbacks = this.listeners.get(event)!;
    callbacks.push(callback);

    // Return unsubscribe function
    return () => {
      const index = callbacks.indexOf(callback);
      if (index > -1) {
        callbacks.splice(index, 1);
      }
    };
  }

  /**
   * Gets connection status
   */
  public isConnected(): boolean {
    return this.ws !== null && this.ws.readyState === WebSocket.OPEN;
  }

  private emit(event: string, message: TelemetryMessage): void {
    const callbacks = this.listeners.get(event) || [];
    callbacks.forEach((callback) => {
      try {
        callback(message);
      } catch (error) {
        console.error(`Error in ${event} callback:`, error);
      }
    });
  }

  private startHeartbeat(): void {
    this.heartbeatTimer = window.setInterval(() => {
      if (this.isConnected()) {
        this.send({ type: 'ping', timestamp: Date.now() });
      }
    }, this.config.heartbeatInterval);
  }

  private stopHeartbeat(): void {
    if (this.heartbeatTimer !== null) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = null;
    }
  }

  private attemptReconnect(): void {
    if (this.reconnectAttempts < this.config.reconnectAttempts) {
      this.reconnectAttempts++;
      const delay = this.config.reconnectDelay * this.reconnectAttempts;
      console.log(`📡 Reconnecting in ${delay}ms... (attempt ${this.reconnectAttempts})`);

      setTimeout(() => {
        this.connect().catch((error) => {
          console.error('Reconnection failed:', error);
        });
      }, delay);
    } else {
      console.error('❌ Max reconnection attempts reached');
      this.emit('error', {
        type: 'error',
        data: { message: 'Failed to reconnect to Telemetry Bridge' },
        timestamp: Date.now(),
      });
    }
  }
}

/**
 * Singleton instance for global telemetry service
 */
export const telemetryService = new TelemetryWebSocketService({
  url: process.env.EXPO_PUBLIC_WS_URL,
});
