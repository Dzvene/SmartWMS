/**
 * Returns Module Types
 * TypeScript interfaces for Returns API endpoints
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type ReturnOrderStatus =
  | 'Pending'
  | 'InTransit'
  | 'Received'
  | 'InProgress'
  | 'Complete'
  | 'Cancelled';

export type ReturnType =
  | 'CustomerReturn'
  | 'SupplierReturn'
  | 'InternalTransfer'
  | 'Damaged'
  | 'Recall';

export type ReturnCondition =
  | 'Unknown'
  | 'Good'
  | 'Refurbished'
  | 'Damaged'
  | 'Defective'
  | 'Destroyed';

export type ReturnDisposition =
  | 'Pending'
  | 'ReturnToStock'
  | 'Quarantine'
  | 'Scrap'
  | 'ReturnToSupplier'
  | 'Donate'
  | 'Repair';

// ============================================================================
// DTOs
// ============================================================================

export interface ReturnOrderDto {
  id: string;
  returnNumber: string;
  originalSalesOrderId?: string;
  originalSalesOrderNumber?: string;
  customerId: string;
  customerName?: string;
  status: ReturnOrderStatus;
  returnType: ReturnType;
  reasonCodeId?: string;
  reasonDescription?: string;
  receivingLocationId?: string;
  receivingLocationCode?: string;
  requestedDate?: string;
  receivedDate?: string;
  processedDate?: string;
  rmaNumber?: string;
  rmaExpiryDate?: string;
  carrierCode?: string;
  trackingNumber?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  totalLines: number;
  totalQuantityExpected: number;
  totalQuantityReceived: number;
  receivingProgress: number;
  notes?: string;
  internalNotes?: string;
  lines: ReturnOrderLineDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface ReturnOrderListDto {
  id: string;
  returnNumber: string;
  originalSalesOrderNumber?: string;
  customerName?: string;
  status: ReturnOrderStatus;
  returnType: ReturnType;
  rmaNumber?: string;
  totalLines: number;
  totalQuantityExpected: number;
  totalQuantityReceived: number;
  requestedDate?: string;
  receivedDate?: string;
  createdAt: string;
}

export interface ReturnOrderLineDto {
  id: string;
  returnOrderId: string;
  lineNumber: number;
  productId: string;
  productName?: string;
  sku: string;
  quantityExpected: number;
  quantityReceived: number;
  quantityAccepted: number;
  quantityRejected: number;
  condition: ReturnCondition;
  conditionNotes?: string;
  disposition: ReturnDisposition;
  dispositionLocationId?: string;
  dispositionLocationCode?: string;
  batchNumber?: string;
  serialNumber?: string;
  originalOrderLineId?: string;
  reasonCodeId?: string;
  reasonDescription?: string;
  notes?: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface ReturnOrderFilters {
  status?: ReturnOrderStatus;
  returnType?: ReturnType;
  customerId?: string;
  originalSalesOrderId?: string;
  assignedToUserId?: string;
  rmaNumber?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateReturnOrderRequest {
  originalSalesOrderId?: string;
  customerId: string;
  returnType?: ReturnType;
  reasonCodeId?: string;
  reasonDescription?: string;
  receivingLocationId?: string;
  requestedDate?: string;
  rmaNumber?: string;
  rmaExpiryDate?: string;
  notes?: string;
  lines?: CreateReturnOrderLineRequest[];
}

export interface CreateReturnOrderLineRequest {
  productId: string;
  sku: string;
  quantityExpected: number;
  batchNumber?: string;
  serialNumber?: string;
  originalOrderLineId?: string;
  reasonCodeId?: string;
  reasonDescription?: string;
  notes?: string;
}

export interface UpdateReturnOrderRequest {
  reasonCodeId?: string;
  reasonDescription?: string;
  receivingLocationId?: string;
  rmaNumber?: string;
  rmaExpiryDate?: string;
  carrierCode?: string;
  trackingNumber?: string;
  notes?: string;
  internalNotes?: string;
}

export interface AddReturnLineRequest {
  productId: string;
  sku: string;
  quantityExpected: number;
  batchNumber?: string;
  serialNumber?: string;
  reasonCodeId?: string;
  reasonDescription?: string;
  notes?: string;
}

export interface ReceiveReturnLineRequest {
  quantityReceived: number;
  condition: ReturnCondition;
  conditionNotes?: string;
  batchNumber?: string;
  serialNumber?: string;
}

export interface ProcessReturnLineRequest {
  quantityAccepted: number;
  quantityRejected: number;
  disposition: ReturnDisposition;
  dispositionLocationId?: string;
  notes?: string;
}

export interface AssignReturnRequest {
  userId: string;
}

export interface SetReturnShippingRequest {
  carrierCode?: string;
  trackingNumber: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type ReturnOrderResponse = ApiResponse<ReturnOrderDto>;
export type ReturnOrderListResponse = ApiResponse<PaginatedResponse<ReturnOrderListDto>>;
