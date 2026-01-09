/**
 * Transfers Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

export type TransferStatus =
  | 'Draft'
  | 'Released'
  | 'InProgress'
  | 'PartiallyCompleted'
  | 'Completed'
  | 'Cancelled';

export type TransferType = 'Internal' | 'InterWarehouse' | 'Replenishment' | 'Relocation' | 'Consolidation';

export type TransferPriority = 'Low' | 'Normal' | 'High' | 'Urgent';

export type TransferLineStatus = 'Pending' | 'PartiallyPicked' | 'Picked' | 'InTransit' | 'Received' | 'Cancelled';

export interface StockTransferDto {
  id: string;
  transferNumber: string;
  transferType: TransferType;
  status: TransferStatus;
  priority: TransferPriority;
  sourceWarehouseId: string;
  sourceWarehouseName?: string;
  destinationWarehouseId: string;
  destinationWarehouseName?: string;
  sourceLocationId?: string;
  sourceLocationCode?: string;
  destinationLocationId?: string;
  destinationLocationCode?: string;
  requestedDate?: string;
  scheduledDate?: string;
  startedAt?: string;
  completedAt?: string;
  requestedByUserId?: string;
  requestedByUserName?: string;
  assignedUserId?: string;
  assignedUserName?: string;
  notes?: string;
  totalLines: number;
  completedLines: number;
  totalQuantity: number;
  transferredQuantity: number;
  lines?: StockTransferLineDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface StockTransferLineDto {
  id: string;
  transferId: string;
  lineNumber: number;
  productId: string;
  sku: string;
  productName?: string;
  sourceLocationId: string;
  sourceLocationCode?: string;
  destinationLocationId: string;
  destinationLocationCode?: string;
  batchNumber?: string;
  serialNumber?: string;
  requestedQuantity: number;
  pickedQuantity: number;
  receivedQuantity: number;
  status: TransferLineStatus;
  pickedByUserId?: string;
  pickedByUserName?: string;
  pickedAt?: string;
  receivedByUserId?: string;
  receivedByUserName?: string;
  receivedAt?: string;
  notes?: string;
}

export interface StockTransferSummaryDto {
  id: string;
  transferNumber: string;
  transferType: TransferType;
  status: TransferStatus;
  priority: TransferPriority;
  sourceWarehouseName?: string;
  destinationWarehouseName?: string;
  scheduledDate?: string;
  assignedUserName?: string;
  totalLines: number;
  completedLines: number;
  createdAt: string;
}

export interface TransferFilters {
  sourceWarehouseId?: string;
  destinationWarehouseId?: string;
  transferType?: TransferType;
  status?: TransferStatus;
  priority?: TransferPriority;
  assignedUserId?: string;
  dateFrom?: string;
  dateTo?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateTransferRequest {
  transferType: TransferType;
  priority?: TransferPriority;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  sourceLocationId?: string;
  destinationLocationId?: string;
  scheduledDate?: string;
  notes?: string;
  lines?: CreateTransferLineRequest[];
}

export interface CreateTransferLineRequest {
  productId: string;
  sku: string;
  sourceLocationId: string;
  destinationLocationId: string;
  batchNumber?: string;
  serialNumber?: string;
  requestedQuantity: number;
  notes?: string;
}

export interface PickTransferLineRequest {
  lineId: string;
  pickedQuantity: number;
  notes?: string;
}

export interface ReceiveTransferLineRequest {
  lineId: string;
  receivedQuantity: number;
  notes?: string;
}

export type StockTransferResponse = ApiResponse<StockTransferDto>;
export type StockTransferListResponse = ApiResponse<PaginatedResponse<StockTransferSummaryDto>>;
