/**
 * Carriers API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Carriers/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type CarrierIntegrationType = 'Manual' | 'API' | 'EDI' | 'File';

export type ServiceType =
  | 'Ground'
  | 'Express'
  | 'Overnight'
  | 'SameDay'
  | 'Freight'
  | 'International'
  | 'Economy';

// ============================================================================
// Carrier Types
// ============================================================================

export interface CarrierDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  contactName?: string;
  phone?: string;
  email?: string;
  website?: string;
  accountNumber?: string;
  integrationType: CarrierIntegrationType;
  isActive: boolean;
  defaultServiceCode?: string;
  notes?: string;
  services: CarrierServiceDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface CarrierListDto {
  id: string;
  code: string;
  name: string;
  integrationType: CarrierIntegrationType;
  isActive: boolean;
  serviceCount: number;
  createdAt: string;
}

export interface CarrierServiceDto {
  id: string;
  carrierId: string;
  code: string;
  name: string;
  description?: string;
  minTransitDays?: number;
  maxTransitDays?: number;
  serviceType: ServiceType;
  hasTracking: boolean;
  trackingUrlTemplate?: string;
  maxWeightKg?: number;
  maxLengthCm?: number;
  maxWidthCm?: number;
  maxHeightCm?: number;
  isActive: boolean;
  notes?: string;
  createdAt: string;
}

export interface CreateCarrierRequest {
  code: string;
  name: string;
  description?: string;
  contactName?: string;
  phone?: string;
  email?: string;
  website?: string;
  accountNumber?: string;
  integrationType?: CarrierIntegrationType;
  defaultServiceCode?: string;
  notes?: string;
}

export interface UpdateCarrierRequest {
  name: string;
  description?: string;
  contactName?: string;
  phone?: string;
  email?: string;
  website?: string;
  accountNumber?: string;
  integrationType: CarrierIntegrationType;
  isActive: boolean;
  defaultServiceCode?: string;
  notes?: string;
}

export interface CreateCarrierServiceRequest {
  code: string;
  name: string;
  description?: string;
  minTransitDays?: number;
  maxTransitDays?: number;
  serviceType?: ServiceType;
  hasTracking?: boolean;
  trackingUrlTemplate?: string;
  maxWeightKg?: number;
  maxLengthCm?: number;
  maxWidthCm?: number;
  maxHeightCm?: number;
  notes?: string;
}

export interface UpdateCarrierServiceRequest {
  name: string;
  description?: string;
  minTransitDays?: number;
  maxTransitDays?: number;
  serviceType: ServiceType;
  hasTracking: boolean;
  trackingUrlTemplate?: string;
  maxWeightKg?: number;
  maxLengthCm?: number;
  maxWidthCm?: number;
  maxHeightCm?: number;
  isActive: boolean;
  notes?: string;
}

export interface CarrierFilters extends PaginationParams {
  search?: string;
  integrationType?: CarrierIntegrationType;
  isActive?: boolean;
}

// ============================================================================
// Response Types
// ============================================================================

export type CarrierResponse = ApiResponse<CarrierDto>;
export type CarrierListResponse = ApiResponse<PaginatedResponse<CarrierListDto>>;
export type CarrierServiceResponse = ApiResponse<CarrierServiceDto>;
