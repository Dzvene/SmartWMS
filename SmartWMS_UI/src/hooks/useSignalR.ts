import { useEffect, useRef } from 'react';
import { useSelector } from 'react-redux';
import signalRService, { WarehouseEventType } from '../api/signalr';
import type { RootState } from '../store';

/**
 * Hook to connect to SignalR on authentication
 */
export function useSignalRConnection() {
  const token = useSelector((state: RootState) => state.auth?.token);
  const isConnected = useRef(false);

  useEffect(() => {
    if (token && !isConnected.current) {
      signalRService
        .connect(token)
        .then(() => {
          isConnected.current = true;
        })
        .catch((err) => {
          console.error('Failed to connect to SignalR:', err);
        });
    }

    return () => {
      if (isConnected.current) {
        signalRService.disconnect();
        isConnected.current = false;
      }
    };
  }, [token]);
}

/**
 * Hook to subscribe to SignalR events
 */
export function useSignalREvent<T = unknown>(
  eventType: WarehouseEventType,
  callback: (data: T) => void
) {
  const callbackRef = useRef(callback);
  callbackRef.current = callback;

  useEffect(() => {
    const handler = (data: unknown) => {
      callbackRef.current(data as T);
    };

    const unsubscribe = signalRService.on(eventType, handler);
    return unsubscribe;
  }, [eventType]);
}

/**
 * Hook for order-related real-time updates
 */
export function useOrderUpdates(
  onCreated?: (order: unknown) => void,
  onUpdated?: (order: unknown) => void,
  onStatusChanged?: (data: { orderId: string; oldStatus: string; newStatus: string }) => void
) {
  useSignalREvent('OrderCreated', (data) => onCreated?.(data));
  useSignalREvent('OrderUpdated', (data) => onUpdated?.(data));
  useSignalREvent('OrderStatusChanged', (data) =>
    onStatusChanged?.(data as { orderId: string; oldStatus: string; newStatus: string })
  );
}

/**
 * Hook for task-related real-time updates
 */
export function useTaskUpdates(
  onAssigned?: (task: unknown) => void,
  onCompleted?: (task: unknown) => void
) {
  useSignalREvent('TaskAssigned', (data) => onAssigned?.(data));
  useSignalREvent('TaskCompleted', (data) => onCompleted?.(data));
}

/**
 * Hook for stock-related real-time updates
 */
export function useStockUpdates(
  onChanged?: (data: { productId: string; locationId: string; newQuantity: number }) => void,
  onLowStock?: (data: {
    productId: string;
    sku: string;
    currentQuantity: number;
    minimumQuantity: number;
  }) => void
) {
  useSignalREvent('StockChanged', (data) =>
    onChanged?.(data as { productId: string; locationId: string; newQuantity: number })
  );
  useSignalREvent('LowStockAlert', (data) =>
    onLowStock?.(
      data as {
        productId: string;
        sku: string;
        currentQuantity: number;
        minimumQuantity: number;
      }
    )
  );
}

/**
 * Hook for entity subscription (e.g., watching a specific order)
 */
export function useEntitySubscription(entityType: string, entityId: string | undefined) {
  useEffect(() => {
    if (!entityId) return;

    signalRService.subscribeToEntity(entityType, entityId);

    return () => {
      signalRService.unsubscribeFromEntity(entityType, entityId);
    };
  }, [entityType, entityId]);
}

/**
 * Hook for notifications
 */
export function useNotifications(onNotification: (notification: unknown) => void) {
  useSignalREvent('Notification', onNotification);
}
