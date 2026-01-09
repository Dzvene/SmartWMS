/**
 * Packing API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Packing/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type PackingTaskStatus = 'Pending' | 'Assigned' | 'InProgress' | 'Completed' | 'Cancelled';

// ============================================================================
// Packing Task Types
// ============================================================================

export interface PackingTaskDto {
  id: string;
  taskNumber: string;

  salesOrderId: string;
  salesOrderNumber?: string;
  customerName?: string;

  fulfillmentBatchId?: string;
  fulfillmentBatchNumber?: string;

  packingStationId?: string;
  packingStationCode?: string;
  packingStationName?: string;

  assignedToUserId?: string;
  assignedToUserName?: string;
  assignedAt?: string;

  status: PackingTaskStatus;

  totalItems: number;
  packedItems: number;
  packingProgress: number;

  boxCount: number;
  totalWeightKg: number;

  startedAt?: string;
  completedAt?: string;

  priority: number;
  notes?: string;

  packages?: PackageDto[];

  createdAt: string;
  updatedAt?: string;
}

export interface PackingTaskListDto {
  id: string;
  taskNumber: string;
  salesOrderNumber?: string;
  customerName?: string;
  packingStationCode?: string;
  assignedToUserName?: string;
  status: PackingTaskStatus;
  totalItems: number;
  packedItems: number;
  boxCount: number;
  priority: number;
  startedAt?: string;
  completedAt?: string;
  createdAt: string;
}

export interface PackageDto {
  id: string;
  packingTaskId: string;
  packageNumber: string;
  sequenceNumber: number;

  lengthMm?: number;
  widthMm?: number;
  heightMm?: number;
  volumeCm3?: number;

  weightKg: number;
  packagingType?: string;

  trackingNumber?: string;
  labelUrl?: string;

  items?: PackageItemDto[];

  createdAt: string;
}

export interface PackageItemDto {
  id: string;
  packageId: string;
  productId: string;
  productName?: string;
  sku: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
}

export interface PackingStationDto {
  id: string;
  code: string;
  name: string;
  warehouseId: string;
  warehouseName?: string;
  isActive: boolean;
  canPrintLabels: boolean;
  hasScale: boolean;
  hasDimensioner: boolean;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface CreatePackingTaskRequest {
  salesOrderId: string;
  fulfillmentBatchId?: string;
  packingStationId?: string;
  assignToUserId?: string;
  priority?: number;
  notes?: string;
}

export interface AssignPackingTaskRequest {
  userId: string;
}

export interface CreatePackageRequest {
  lengthMm?: number;
  widthMm?: number;
  heightMm?: number;
  weightKg: number;
  packagingType?: string;
}

export interface AddItemToPackageRequest {
  productId: string;
  sku: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
}

export interface CompletePackingTaskRequest {
  notes?: string;
}

export interface PackingTaskFilters extends PaginationParams {
  search?: string;
  status?: PackingTaskStatus;
  assignedToUserId?: string;
  salesOrderId?: string;
  fulfillmentBatchId?: string;
  packingStationId?: string;
  warehouseId?: string;
  priority?: number;
  unassigned?: boolean;
}

// ============================================================================
// Response Types
// ============================================================================

export type PackingTaskResponse = ApiResponse<PackingTaskDto>;
export type PackingTaskListResponse = ApiResponse<PaginatedResponse<PackingTaskListDto>>;
export type PackageResponse = ApiResponse<PackageDto>;
export type PackingStationResponse = ApiResponse<PackingStationDto>;
export type PackingStationListResponse = ApiResponse<PaginatedResponse<PackingStationDto>>;
