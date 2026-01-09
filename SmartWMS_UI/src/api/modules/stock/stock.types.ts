/**
 * Stock Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type MovementType =
  | 'Receive'
  | 'Issue'
  | 'Transfer'
  | 'Adjustment'
  | 'Return'
  | 'Scrap'
  | 'Reserve'
  | 'ReleaseReservation';

// ============================================================================
// DTOs
// ============================================================================

export interface StockLevelDto {
  id: string;
  productId: string;
  sku?: string;
  productName?: string;
  locationId: string;
  locationCode?: string;
  warehouseName?: string;
  zoneName?: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  batchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;
  lastMovementAt?: string;
  lastCountAt?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ProductStockSummaryDto {
  productId: string;
  sku?: string;
  productName?: string;
  totalOnHand: number;
  totalReserved: number;
  totalAvailable: number;
  locationCount: number;
  locations?: StockLevelDto[];
}

export interface StockMovementDto {
  id: string;
  movementNumber?: string;
  movementType: MovementType;
  productId: string;
  sku?: string;
  productName?: string;
  fromLocationId?: string;
  fromLocationCode?: string;
  toLocationId?: string;
  toLocationCode?: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
  referenceType?: string;
  referenceId?: string;
  referenceNumber?: string;
  reasonCode?: string;
  notes?: string;
  movementDate: string;
  createdAt: string;
}

export interface LowStockDto {
  productId: string;
  sku: string;
  productName: string;
  currentStock: number;
  reorderPoint: number;
  reorderQuantity: number;
  warehouseName?: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface StockLevelFilters {
  productId?: string;
  locationId?: string;
  warehouseId?: string;
  zoneId?: string;
  sku?: string;
  batchNumber?: string;
  hasAvailableStock?: boolean;
  isExpiringSoon?: boolean;
  expiringWithinDays?: number;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface StockMovementFilters {
  productId?: string;
  locationId?: string;
  movementType?: MovementType;
  referenceType?: string;
  referenceId?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface ReceiveStockRequest {
  productId: string;
  locationId: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;
  referenceType?: string;
  referenceId?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface IssueStockRequest {
  productId: string;
  locationId: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
  referenceType?: string;
  referenceId?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface TransferStockRequest {
  productId: string;
  fromLocationId: string;
  toLocationId: string;
  quantity: number;
  batchNumber?: string;
  serialNumber?: string;
  reasonCode?: string;
  notes?: string;
}

export interface AdjustStockRequest {
  productId: string;
  locationId: string;
  newQuantity: number;
  batchNumber?: string;
  serialNumber?: string;
  reasonCode?: string;
  notes?: string;
}

export interface ReserveStockRequest {
  productId: string;
  locationId: string;
  quantity: number;
  batchNumber?: string;
  referenceType?: string;
  referenceId?: string;
  referenceNumber?: string;
}

export interface ReleaseReservationRequest {
  productId: string;
  locationId: string;
  quantity: number;
  batchNumber?: string;
  referenceId?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type StockLevelResponse = ApiResponse<StockLevelDto>;
export type StockLevelListResponse = ApiResponse<PaginatedResponse<StockLevelDto>>;
export type ProductStockSummaryResponse = ApiResponse<ProductStockSummaryDto>;
export type StockMovementApiResponse = ApiResponse<StockMovementDto>;
export type StockMovementListResponse = ApiResponse<PaginatedResponse<StockMovementDto>>;
export type LowStockListResponse = ApiResponse<LowStockDto[]>;
export type AvailableQuantityResponse = ApiResponse<{ available: number }>;
