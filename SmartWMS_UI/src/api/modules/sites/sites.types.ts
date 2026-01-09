import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================
// Site DTOs
// ============================================

export interface SiteDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  timezone?: string;
  isActive: boolean;
  isPrimary: boolean;
  warehousesCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateSiteRequest {
  code: string;
  name: string;
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

export interface UpdateSiteRequest {
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

export interface SitesListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export type SitesListResponse = ApiResponse<PaginatedResponse<SiteDto>>;
export type SiteDetailResponse = ApiResponse<SiteDto>;
