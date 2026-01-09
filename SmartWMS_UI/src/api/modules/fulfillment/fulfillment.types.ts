/**
 * Fulfillment API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Fulfillment/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type PickTaskStatus =
  | 'Pending'
  | 'Assigned'
  | 'InProgress'
  | 'Completed'
  | 'ShortPicked'
  | 'Cancelled';

export type BatchStatus =
  | 'Open'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled';

export type ShipmentStatus =
  | 'Pending'
  | 'ReadyToShip'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled';

// ============================================================================
// Pick Task Types
// ============================================================================

export interface PickTaskDto {
  id: string;
  taskNumber: string;

  // Batch
  batchId?: string;
  batchNumber?: string;

  // Order
  orderId: string;
  orderNumber?: string;
  orderLineId: string;

  // Product
  productId: string;
  sku: string;
  productName?: string;

  // Location
  fromLocationId: string;
  fromLocationCode?: string;
  toLocationId?: string;
  toLocationCode?: string;

  // Quantities
  quantityRequired: number;
  quantityPicked: number;
  quantityShortPicked: number;
  quantityOutstanding: number;

  // Batch/Serial
  pickedBatchNumber?: string;
  pickedSerialNumber?: string;

  // Status
  status: PickTaskStatus;

  // Priority and sequence
  priority: number;
  sequence: number;

  // Assignment
  assignedToUserId?: string;
  assignedToUserName?: string;

  // Timestamps
  startedAt?: string;
  completedAt?: string;

  // Short pick
  shortPickReason?: string;

  createdAt: string;
  updatedAt?: string;
}

export interface CreatePickTaskRequest {
  batchId?: string;
  orderId: string;
  orderLineId: string;
  productId: string;
  fromLocationId: string;
  toLocationId?: string;
  quantityRequired: number;
  priority?: number;
  assignedToUserId?: string;
}

export interface AssignPickTaskRequest {
  userId?: string;
}

export interface StartPickTaskRequest {
  notes?: string;
}

export interface ConfirmPickRequest {
  quantityPicked: number;
  batchNumber?: string;
  serialNumber?: string;
  toLocationId?: string;
}

export interface ShortPickRequest {
  quantityPicked: number;
  quantityShortPicked: number;
  reason: string;
}

export interface PickTaskFilters extends PaginationParams {
  search?: string;
  status?: PickTaskStatus;
  batchId?: string;
  orderId?: string;
  assignedToUserId?: string;
  fromLocationId?: string;
  warehouseId?: string;
  isAssigned?: boolean;
}

// ============================================================================
// Fulfillment Batch Types
// ============================================================================

export interface FulfillmentBatchDto {
  id: string;
  batchNumber: string;
  warehouseId: string;
  warehouseName?: string;
  status: BatchStatus;
  totalTasks: number;
  completedTasks: number;
  assignedToUserId?: string;
  assignedToUserName?: string;
  startedAt?: string;
  completedAt?: string;
  createdAt: string;
  updatedAt?: string;
  pickTasks?: PickTaskDto[];
}

export interface CreateBatchRequest {
  warehouseId: string;
  orderIds?: string[];
  assignedToUserId?: string;
}

export interface BatchFilters extends PaginationParams {
  search?: string;
  status?: BatchStatus;
  warehouseId?: string;
  assignedToUserId?: string;
}

// ============================================================================
// Shipment Types
// ============================================================================

export interface ShipmentDto {
  id: string;
  shipmentNumber: string;

  // Order
  salesOrderId: string;
  salesOrderNumber?: string;

  // Customer
  customerId?: string;
  customerName?: string;

  // Warehouse
  warehouseId: string;
  warehouseName?: string;

  // Carrier
  carrierId?: string;
  carrierName?: string;
  carrierServiceId?: string;
  carrierServiceName?: string;

  // Status
  status: ShipmentStatus;

  // Shipping details
  trackingNumber?: string;
  trackingUrl?: string;

  // Ship to address
  shipToName?: string;
  shipToAddressLine1?: string;
  shipToAddressLine2?: string;
  shipToCity?: string;
  shipToRegion?: string;
  shipToPostalCode?: string;
  shipToCountryCode?: string;

  // Package info
  packageCount: number;
  totalWeight?: number;
  weightUnit?: string;

  // Dates
  shippedAt?: string;
  deliveredAt?: string;
  estimatedDeliveryDate?: string;

  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateShipmentRequest {
  salesOrderId: string;
  carrierId?: string;
  carrierServiceId?: string;
  trackingNumber?: string;
  packageCount?: number;
  totalWeight?: number;
  weightUnit?: string;
  notes?: string;
}

export interface UpdateShipmentRequest {
  carrierId?: string;
  carrierServiceId?: string;
  trackingNumber?: string;
  packageCount?: number;
  totalWeight?: number;
  weightUnit?: string;
  estimatedDeliveryDate?: string;
  notes?: string;
}

export interface ShipShipmentRequest {
  trackingNumber?: string;
  shippedAt?: string;
}

export interface ShipmentFilters extends PaginationParams {
  search?: string;
  status?: ShipmentStatus;
  warehouseId?: string;
  carrierId?: string;
  salesOrderId?: string;
  shippedDateFrom?: string;
  shippedDateTo?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type PickTaskResponse = ApiResponse<PickTaskDto>;
export type PickTaskListResponse = ApiResponse<PaginatedResponse<PickTaskDto>>;

export type FulfillmentBatchResponse = ApiResponse<FulfillmentBatchDto>;
export type FulfillmentBatchListResponse = ApiResponse<PaginatedResponse<FulfillmentBatchDto>>;

export type ShipmentResponse = ApiResponse<ShipmentDto>;
export type ShipmentListResponse = ApiResponse<PaginatedResponse<ShipmentDto>>;
