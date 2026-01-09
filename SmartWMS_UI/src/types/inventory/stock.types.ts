import { Entity } from '@/models';

/**
 * Stock level at a specific location
 */
export interface StockLevel extends Entity {
  sku: string;
  productName: string;
  locationId: string;
  locationCode: string;

  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;

  batchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;
  lotNumber?: string;

  lastMovementAt?: string;
  lastCountedAt?: string;
}

/**
 * Stock movement/transaction record
 */
export interface StockMovement extends Entity {
  sku: string;
  productName: string;

  fromLocationId?: string;
  fromLocationCode?: string;
  toLocationId?: string;
  toLocationCode?: string;

  quantity: number;
  movementType: MovementType;

  referenceType?: string;
  referenceId?: string;
  referenceNumber?: string;

  batchNumber?: string;
  serialNumber?: string;

  performedBy: string;
  performedAt: string;
  notes?: string;
}

/**
 * Movement type classification
 */
export enum MovementType {
  Receipt = 'RECEIPT',
  Issue = 'ISSUE',
  Transfer = 'TRANSFER',
  Adjustment = 'ADJUSTMENT',
  Return = 'RETURN',
  Write_Off = 'WRITE_OFF',
  Cycle_Count = 'CYCLE_COUNT',
}

/**
 * Stock adjustment request
 */
export interface StockAdjustment {
  sku: string;
  locationId: string;
  quantity: number;
  adjustmentType: 'INCREASE' | 'DECREASE' | 'SET';
  reasonCode: string;
  notes?: string;
  batchNumber?: string;
}

/**
 * Stock reservation for orders
 */
export interface StockReservation extends Entity {
  sku: string;
  locationId: string;
  quantity: number;
  orderType: 'SALES' | 'TRANSFER';
  orderId: string;
  orderLineId: string;
  reservedAt: string;
  expiresAt?: string;
}

/**
 * Stock summary by product
 */
export interface StockSummary {
  sku: string;
  productName: string;
  totalOnHand: number;
  totalReserved: number;
  totalAvailable: number;
  locationCount: number;
  oldestBatch?: string;
  nearestExpiry?: string;
}

/**
 * Stock filter parameters
 */
export interface StockFilter {
  sku?: string;
  locationId?: string;
  warehouseId?: string;
  zoneId?: string;
  hasStock?: boolean;
  expiringBefore?: string;
  batchNumber?: string;
}
