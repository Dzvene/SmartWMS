import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================
// Zone Enums
// ============================================

export type ZoneType =
  | 'Storage'
  | 'Picking'
  | 'Packing'
  | 'Staging'
  | 'Shipping'
  | 'Receiving'
  | 'Returns';

export const ZoneTypeLabels: Record<ZoneType, string> = {
  Storage: 'Storage',
  Picking: 'Picking',
  Packing: 'Packing',
  Staging: 'Staging',
  Shipping: 'Shipping',
  Receiving: 'Receiving',
  Returns: 'Returns',
};

// ============================================
// Zone DTOs
// ============================================

export interface ZoneDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  warehouseId: string;
  warehouseName?: string;
  zoneType: ZoneType;
  pickSequence?: number;
  isActive: boolean;
  locationCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateZoneRequest {
  code: string;
  name: string;
  description?: string;
  warehouseId: string;
  zoneType?: ZoneType;
  pickSequence?: number;
  isActive?: boolean;
}

export interface UpdateZoneRequest {
  name?: string;
  description?: string;
  zoneType?: ZoneType;
  pickSequence?: number;
  isActive?: boolean;
}

// ============================================
// API Request/Response Types
// ============================================

export interface ZonesListParams {
  warehouseId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export type ZonesListResponse = ApiResponse<PaginatedResponse<ZoneDto>>;
export type ZoneDetailResponse = ApiResponse<ZoneDto>;
