import * as signalR from '@microsoft/signalr';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5050';

let connection: signalR.HubConnection | null = null;

export type WarehouseEventType =
  | 'OrderCreated'
  | 'OrderUpdated'
  | 'OrderStatusChanged'
  | 'TaskAssigned'
  | 'TaskCompleted'
  | 'StockChanged'
  | 'LowStockAlert'
  | 'Notification';

type EventCallback = (data: unknown) => void;
const eventHandlers: Map<WarehouseEventType, Set<EventCallback>> = new Map();

export const signalRService = {
  /**
   * Connect to the SignalR hub
   */
  async connect(accessToken: string): Promise<void> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/warehouse`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Register all event handlers
    const eventTypes: WarehouseEventType[] = [
      'OrderCreated',
      'OrderUpdated',
      'OrderStatusChanged',
      'TaskAssigned',
      'TaskCompleted',
      'StockChanged',
      'LowStockAlert',
      'Notification',
    ];

    eventTypes.forEach((eventType) => {
      connection?.on(eventType, (data: unknown) => {
        const handlers = eventHandlers.get(eventType);
        handlers?.forEach((handler) => handler(data));
      });
    });

    connection.onreconnecting(() => {
      console.warn('[SignalR] Reconnecting...');
    });

    connection.onreconnected(() => {
      console.warn('[SignalR] Reconnected');
    });

    connection.onclose(() => {
      console.warn('[SignalR] Connection closed');
    });

    try {
      await connection.start();
    } catch (err) {
      console.error('[SignalR] Connection failed:', err);
      throw err;
    }
  },

  /**
   * Disconnect from the hub
   */
  async disconnect(): Promise<void> {
    if (connection) {
      await connection.stop();
      connection = null;
    }
  },

  /**
   * Subscribe to an event
   */
  on(eventType: WarehouseEventType, callback: EventCallback): () => void {
    if (!eventHandlers.has(eventType)) {
      eventHandlers.set(eventType, new Set());
    }
    eventHandlers.get(eventType)!.add(callback);

    // Return unsubscribe function
    return () => {
      eventHandlers.get(eventType)?.delete(callback);
    };
  },

  /**
   * Subscribe to entity-specific updates
   */
  async subscribeToEntity(entityType: string, entityId: string): Promise<void> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('SubscribeToEntity', entityType, entityId);
    }
  },

  /**
   * Unsubscribe from entity updates
   */
  async unsubscribeFromEntity(entityType: string, entityId: string): Promise<void> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('UnsubscribeFromEntity', entityType, entityId);
    }
  },

  /**
   * Check if connected
   */
  isConnected(): boolean {
    return connection?.state === signalR.HubConnectionState.Connected;
  },

  /**
   * Get connection state
   */
  getState(): signalR.HubConnectionState | null {
    return connection?.state ?? null;
  },
};

export default signalRService;
