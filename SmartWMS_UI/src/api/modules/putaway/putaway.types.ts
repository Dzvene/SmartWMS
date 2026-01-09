/**
 * Putaway API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Putaway/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type PutawayTaskStatus = 'Pending' | 'Assigned' | 'InProgress' | 'Complete' | 'Cancelled';

// ============================================================================
// Putaway Task Types
// ============================================================================

export interface PutawayTaskDto {
  id: string;
  taskNumber: string;

  // Source
  goodsReceiptId?: string;
  goodsReceiptNumber?: string;

  // Product
  productId: string;
  sku?: string;
  productName?: string;

  // Quantity
  quantityToPutaway: number;
  quantityPutaway: number;
  quantityRemaining: number;

  // Locations
  fromLocationId: string;
  fromLocationCode?: string;
  suggestedLocationId?: string;
  suggestedLocationCode?: string;
  actualLocationId?: string;
  actualLocationCode?: string;

  // Batch/Serial
  batchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;

  // Assignment
  assignedToUserId?: string;
  assignedToUserName?: string;
  assignedAt?: string;

  // Status
  status: PutawayTaskStatus;

  // Timing
  startedAt?: string;
  completedAt?: string;

  priority: number;
  notes?: string;

  createdAt: string;
}

export interface CreatePutawayFromReceiptRequest {
  goodsReceiptId: string;
  defaultFromLocationId?: string;
}

export interface CreatePutawayTaskRequest {
  productId: string;
  fromLocationId: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;
  suggestedLocationId?: string;
  priority?: number;
  notes?: string;
}

export interface AssignPutawayTaskRequest {
  userId: string;
}

export interface CompletePutawayTaskRequest {
  actualLocationId: string;
  quantityPutaway: number;
  notes?: string;
}

export interface LocationSuggestionDto {
  locationId: string;
  locationCode?: string;
  zoneName?: string;
  warehouseName?: string;
  score: number;
  reason?: string;
}

export interface SuggestLocationRequest {
  productId: string;
  quantity: number;
  batchNumber?: string;
  expiryDate?: string;
  preferredZoneId?: string;
}

export interface PutawayTaskFilters extends PaginationParams {
  search?: string;
  status?: PutawayTaskStatus;
  assignedToUserId?: string;
  productId?: string;
  fromLocationId?: string;
  goodsReceiptId?: string;
  warehouseId?: string;
  priority?: number;
  unassigned?: boolean;
}

// ============================================================================
// Response Types
// ============================================================================

export type PutawayTaskResponse = ApiResponse<PutawayTaskDto>;
export type PutawayTaskListResponse = ApiResponse<PaginatedResponse<PutawayTaskDto>>;
export type LocationSuggestionResponse = ApiResponse<LocationSuggestionDto[]>;
