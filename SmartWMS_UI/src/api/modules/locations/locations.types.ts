import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================
// Location Enums
// ============================================

export type LocationType =
  | 'Bulk'
  | 'Pick'
  | 'Staging'
  | 'Receiving'
  | 'Shipping'
  | 'Returns'
  | 'Quarantine'
  | 'Reserve';

export const LocationTypeLabels: Record<LocationType, string> = {
  Bulk: 'Bulk Storage',
  Pick: 'Pick Location',
  Staging: 'Staging Area',
  Receiving: 'Receiving Dock',
  Shipping: 'Shipping Dock',
  Returns: 'Returns Area',
  Quarantine: 'Quarantine',
  Reserve: 'Reserve Storage',
};

// ============================================
// Location DTOs
// ============================================

export interface LocationDto {
  id: string;
  code: string;
  name?: string;
  warehouseId: string;
  warehouseName?: string;
  zoneId?: string;
  zoneName?: string;
  aisle?: string;
  rack?: string;
  level?: string;
  position?: string;
  locationType: LocationType;
  widthMm?: number;
  heightMm?: number;
  depthMm?: number;
  maxWeight?: number;
  maxVolume?: number;
  isActive: boolean;
  isPickLocation: boolean;
  isPutawayLocation: boolean;
  isReceivingDock: boolean;
  isShippingDock: boolean;
  pickSequence?: number;
  putawaySequence?: number;
  stockLevelCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateLocationRequest {
  code: string;
  name?: string;
  warehouseId: string;
  zoneId?: string;
  aisle?: string;
  rack?: string;
  level?: string;
  position?: string;
  locationType?: LocationType;
  widthMm?: number;
  heightMm?: number;
  depthMm?: number;
  maxWeight?: number;
  maxVolume?: number;
  isActive?: boolean;
  isPickLocation?: boolean;
  isPutawayLocation?: boolean;
  isReceivingDock?: boolean;
  isShippingDock?: boolean;
  pickSequence?: number;
  putawaySequence?: number;
}

export interface UpdateLocationRequest {
  name?: string;
  zoneId?: string;
  aisle?: string;
  rack?: string;
  level?: string;
  position?: string;
  locationType?: LocationType;
  widthMm?: number;
  heightMm?: number;
  depthMm?: number;
  maxWeight?: number;
  maxVolume?: number;
  isActive?: boolean;
  isPickLocation?: boolean;
  isPutawayLocation?: boolean;
  isReceivingDock?: boolean;
  isShippingDock?: boolean;
  pickSequence?: number;
  putawaySequence?: number;
}

// ============================================
// API Request/Response Types
// ============================================

export interface LocationsListParams {
  warehouseId?: string;
  zoneId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export type LocationsListResponse = ApiResponse<PaginatedResponse<LocationDto>>;
export type LocationDetailResponse = ApiResponse<LocationDto>;
