import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================
// Warehouse DTOs
// ============================================

export interface WarehouseDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  siteId: string;
  siteName?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  timezone?: string;
  isPrimary: boolean;
  isActive: boolean;
  zoneCount: number;
  locationCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface WarehouseOptionDto {
  id: string;
  code: string;
  name: string;
}

export interface CreateWarehouseRequest {
  code: string;
  name: string;
  description?: string;
  siteId: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  timezone?: string;
  isPrimary?: boolean;
  isActive?: boolean;
}

export interface UpdateWarehouseRequest {
  name?: string;
  description?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  timezone?: string;
  isPrimary?: boolean;
  isActive?: boolean;
}

// ============================================
// API Request/Response Types
// ============================================

export interface WarehousesListParams {
  siteId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export interface WarehouseOptionsParams {
  siteId?: string;
}

export type WarehousesListResponse = ApiResponse<PaginatedResponse<WarehouseDto>>;
export type WarehouseOptionsResponse = ApiResponse<WarehouseOptionDto[]>;
export type WarehouseDetailResponse = ApiResponse<WarehouseDto>;
