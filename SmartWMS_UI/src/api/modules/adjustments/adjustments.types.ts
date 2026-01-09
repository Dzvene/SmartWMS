/**
 * Adjustments Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

export type AdjustmentStatus = 'Draft' | 'PendingApproval' | 'Approved' | 'Posted' | 'Cancelled';

export type AdjustmentType =
  | 'Correction'
  | 'CycleCount'
  | 'Damage'
  | 'Scrap'
  | 'Found'
  | 'Lost'
  | 'Expiry'
  | 'QualityHold'
  | 'Revaluation'
  | 'Opening'
  | 'Other';

export interface StockAdjustmentDto {
  id: string;
  adjustmentNumber: string;
  warehouseId: string;
  warehouseName?: string;
  status: AdjustmentStatus;
  adjustmentType: AdjustmentType;
  reasonCodeId?: string;
  reasonCodeName?: string;
  reasonNotes?: string;
  createdByUserId: string;
  createdByUserName?: string;
  approvedByUserId?: string;
  approvedByUserName?: string;
  approvedAt?: string;
  postedByUserId?: string;
  postedByUserName?: string;
  postedAt?: string;
  notes?: string;
  totalLines: number;
  totalQuantityChange: number;
  totalValueChange?: number;
  createdAt: string;
  updatedAt?: string;
  lines?: StockAdjustmentLineDto[];
}

export interface StockAdjustmentLineDto {
  id: string;
  adjustmentId: string;
  lineNumber: number;
  productId: string;
  sku: string;
  productName?: string;
  locationId: string;
  locationCode?: string;
  batchNumber?: string;
  serialNumber?: string;
  quantityBefore: number;
  quantityAdjustment: number;
  quantityAfter: number;
  unitCost?: number;
  valueChange?: number;
  reasonCodeId?: string;
  reasonCodeName?: string;
  reasonNotes?: string;
  isProcessed: boolean;
  processedAt?: string;
}

export interface StockAdjustmentSummaryDto {
  id: string;
  adjustmentNumber: string;
  warehouseName?: string;
  status: AdjustmentStatus;
  adjustmentType: AdjustmentType;
  reasonCodeName?: string;
  totalLines: number;
  totalQuantityChange: number;
  createdByUserName?: string;
  createdAt: string;
}

export interface AdjustmentFilters {
  warehouseId?: string;
  status?: AdjustmentStatus;
  adjustmentType?: AdjustmentType;
  reasonCodeId?: string;
  dateFrom?: string;
  dateTo?: string;
  createdByUserId?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateStockAdjustmentRequest {
  warehouseId: string;
  adjustmentType: AdjustmentType;
  reasonCodeId?: string;
  reasonNotes?: string;
  notes?: string;
  lines?: CreateAdjustmentLineRequest[];
}

export interface CreateAdjustmentLineRequest {
  productId: string;
  sku: string;
  locationId: string;
  batchNumber?: string;
  serialNumber?: string;
  quantityAdjustment: number;
  unitCost?: number;
  reasonCodeId?: string;
  reasonNotes?: string;
}

export type StockAdjustmentResponse = ApiResponse<StockAdjustmentDto>;
export type StockAdjustmentListResponse = ApiResponse<PaginatedResponse<StockAdjustmentSummaryDto>>;
