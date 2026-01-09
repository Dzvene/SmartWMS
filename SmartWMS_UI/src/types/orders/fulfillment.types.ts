import { Entity, UserRef } from '@/models';

/**
 * Fulfillment batch/wave grouping orders for processing
 */
export interface FulfillmentBatch extends Entity {
  batchNumber: string;
  name?: string;

  warehouseId: string;
  warehouseCode: string;

  status: FulfillmentStatus;
  batchType: BatchType;

  orderCount: number;
  lineCount: number;
  totalQuantity: number;
  pickedQuantity: number;

  startedAt?: string;
  completedAt?: string;

  assignedTo?: UserRef;
  createdBy?: UserRef;
  createdAt: string;

  priority: number;
  notes?: string;
}

/**
 * Fulfillment batch status
 */
export enum FulfillmentStatus {
  Created = 'CREATED',
  Released = 'RELEASED',
  InProgress = 'IN_PROGRESS',
  PartiallyComplete = 'PARTIALLY_COMPLETE',
  Complete = 'COMPLETE',
  Cancelled = 'CANCELLED',
}

/**
 * Batch type classification
 */
export enum BatchType {
  Single = 'SINGLE',
  Multi = 'MULTI',
  Zone = 'ZONE',
  Wave = 'WAVE',
}

/**
 * Pick task for warehouse worker
 */
export interface PickTask extends Entity {
  taskNumber: string;
  batchId?: string;
  orderId: string;
  orderNumber: string;
  lineId: string;

  sku: string;
  productName: string;

  fromLocationId: string;
  fromLocationCode: string;
  toLocationId?: string;
  toLocationCode?: string;

  quantityRequired: number;
  quantityPicked: number;

  status: TaskStatus;
  priority: number;
  sequence: number;

  batchNumber?: string;
  serialNumbers?: string[];

  assignedTo?: UserRef;
  startedAt?: string;
  completedAt?: string;
}

/**
 * Task status
 */
export enum TaskStatus {
  Pending = 'PENDING',
  Assigned = 'ASSIGNED',
  InProgress = 'IN_PROGRESS',
  Complete = 'COMPLETE',
  ShortPicked = 'SHORT_PICKED',
  Cancelled = 'CANCELLED',
}

/**
 * Delivery/shipment record
 */
export interface Shipment extends Entity {
  shipmentNumber: string;
  orderIds: string[];

  carrierId: string;
  carrierName: string;
  serviceType?: string;

  trackingNumber?: string;
  trackingUrl?: string;

  status: ShipmentStatus;

  packageCount: number;
  totalWeight?: number;

  shipDate?: string;
  deliveryDate?: string;
  estimatedDelivery?: string;

  shippingCost?: number;
  currencyCode?: string;

  notes?: string;
}

/**
 * Shipment status
 */
export enum ShipmentStatus {
  Created = 'CREATED',
  Manifested = 'MANIFESTED',
  PickedUp = 'PICKED_UP',
  InTransit = 'IN_TRANSIT',
  OutForDelivery = 'OUT_FOR_DELIVERY',
  Delivered = 'DELIVERED',
  Failed = 'FAILED',
  Returned = 'RETURNED',
}

/**
 * Fulfillment batch filter
 */
export interface FulfillmentFilter {
  status?: FulfillmentStatus | FulfillmentStatus[];
  warehouseId?: string;
  batchType?: BatchType;
  assignedTo?: number;
  createdFrom?: string;
  createdTo?: string;
}
