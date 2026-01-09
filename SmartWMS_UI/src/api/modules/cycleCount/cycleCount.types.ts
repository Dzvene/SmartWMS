/**
 * CycleCount Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

export type CycleCountStatus =
  | 'Scheduled'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'PartiallyCompleted';

export type CountType = 'Full' | 'Cycle' | 'Spot' | 'ABC' | 'Random';

export type CountScope = 'Location' | 'Product' | 'Category' | 'Zone' | 'Warehouse';

export type CountItemStatus = 'Pending' | 'Counted' | 'Verified' | 'Adjusted' | 'Skipped';

export interface CycleCountSessionDto {
  id: string;
  countNumber: string;
  warehouseId: string;
  warehouseName?: string;
  countType: CountType;
  countScope: CountScope;
  status: CycleCountStatus;
  scheduledDate?: string;
  startedAt?: string;
  completedAt?: string;
  assignedUserId?: string;
  assignedUserName?: string;
  createdByUserId: string;
  createdByUserName?: string;
  totalItems: number;
  countedItems: number;
  varianceItems: number;
  notes?: string;
  items?: CycleCountItemDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface CycleCountItemDto {
  id: string;
  sessionId: string;
  productId: string;
  sku: string;
  productName?: string;
  locationId: string;
  locationCode?: string;
  batchNumber?: string;
  serialNumber?: string;
  expectedQuantity: number;
  countedQuantity?: number;
  variance?: number;
  variancePercentage?: number;
  status: CountItemStatus;
  countedByUserId?: string;
  countedByUserName?: string;
  countedAt?: string;
  notes?: string;
}

export interface CycleCountSessionListDto {
  id: string;
  countNumber: string;
  warehouseName?: string;
  countType: CountType;
  status: CycleCountStatus;
  scheduledDate?: string;
  assignedUserName?: string;
  totalItems: number;
  countedItems: number;
  varianceItems: number;
  createdAt: string;
}

export interface CycleCountFilters {
  warehouseId?: string;
  status?: CycleCountStatus;
  countType?: CountType;
  assignedUserId?: string;
  dateFrom?: string;
  dateTo?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateCycleCountRequest {
  warehouseId: string;
  countType: CountType;
  countScope: CountScope;
  scheduledDate?: string;
  assignedUserId?: string;
  notes?: string;
  locationIds?: string[];
  productIds?: string[];
  zoneIds?: string[];
}

export interface RecordCountRequest {
  itemId: string;
  countedQuantity: number;
  notes?: string;
}

export type CycleCountSessionResponse = ApiResponse<CycleCountSessionDto>;
export type CycleCountSessionListResponse = ApiResponse<PaginatedResponse<CycleCountSessionListDto>>;
