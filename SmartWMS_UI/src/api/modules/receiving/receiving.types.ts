/**
 * Receiving API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Receiving/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type GoodsReceiptStatus =
  | 'Draft'
  | 'Pending'
  | 'InProgress'
  | 'Completed'
  | 'PartiallyReceived'
  | 'Cancelled';

export type GoodsReceiptLineStatus =
  | 'Pending'
  | 'PartiallyReceived'
  | 'Received'
  | 'Rejected';

// ============================================================================
// Goods Receipt Types
// ============================================================================

export interface GoodsReceiptDto {
  id: string;
  receiptNumber: string;

  // Source PO
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;

  // Supplier
  supplierId?: string;
  supplierName?: string;

  // Warehouse
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;

  // Receiving location
  receivingLocationId?: string;
  receivingLocationCode?: string;

  // Status
  status: GoodsReceiptStatus;

  // Dates
  receiptDate: string;
  completedAt?: string;

  // Carrier
  carrierName?: string;
  trackingNumber?: string;
  deliveryNote?: string;

  // Totals
  totalLines: number;
  totalQuantityExpected: number;
  totalQuantityReceived: number;

  // Progress
  progressPercent: number;

  notes?: string;
  createdAt: string;
  updatedAt?: string;

  // Lines
  lines?: GoodsReceiptLineDto[];
}

export interface GoodsReceiptLineDto {
  id: string;
  receiptId: string;
  lineNumber: number;

  // Product
  productId: string;
  sku?: string;
  productName?: string;

  // Quantities
  quantityExpected: number;
  quantityReceived: number;
  quantityRejected: number;
  quantityRemaining: number;

  // Lot/Batch
  batchNumber?: string;
  lotNumber?: string;
  expirationDate?: string;

  // Put-away
  putawayLocationId?: string;
  putawayLocationCode?: string;

  // Status
  status: GoodsReceiptLineStatus;
  qualityStatus?: string;
  rejectionReason?: string;

  // Source PO line
  purchaseOrderLineId?: string;

  notes?: string;
}

export interface CreateGoodsReceiptRequest {
  receiptNumber?: string;
  purchaseOrderId?: string;
  supplierId?: string;
  warehouseId: string;
  receivingLocationId?: string;
  receiptDate?: string;
  carrierName?: string;
  trackingNumber?: string;
  deliveryNote?: string;
  notes?: string;
  lines?: CreateGoodsReceiptLineRequest[];
}

export interface CreateGoodsReceiptLineRequest {
  productId: string;
  quantityExpected: number;
  batchNumber?: string;
  lotNumber?: string;
  expirationDate?: string;
  purchaseOrderLineId?: string;
  notes?: string;
}

export interface UpdateGoodsReceiptRequest {
  receivingLocationId?: string;
  carrierName?: string;
  trackingNumber?: string;
  deliveryNote?: string;
  notes?: string;
}

export interface ReceiveLineRequest {
  quantityReceived: number;
  quantityRejected?: number;
  batchNumber?: string;
  lotNumber?: string;
  expirationDate?: string;
  manufactureDate?: string;
  putawayLocationId?: string;
  qualityStatus?: string;
  rejectionReason?: string;
  notes?: string;
}

export interface AddGoodsReceiptLineRequest {
  productId: string;
  quantityExpected: number;
  batchNumber?: string;
  lotNumber?: string;
  expirationDate?: string;
  purchaseOrderLineId?: string;
  notes?: string;
}

export interface GoodsReceiptFilters extends PaginationParams {
  search?: string;
  status?: GoodsReceiptStatus;
  warehouseId?: string;
  supplierId?: string;
  purchaseOrderId?: string;
  receiptDateFrom?: string;
  receiptDateTo?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type GoodsReceiptResponse = ApiResponse<GoodsReceiptDto>;
export type GoodsReceiptListResponse = ApiResponse<PaginatedResponse<GoodsReceiptDto>>;
export type GoodsReceiptLineResponse = ApiResponse<GoodsReceiptLineDto>;
