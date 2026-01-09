/**
 * Configuration Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type BarcodePrefixType =
  | 'Product'
  | 'Location'
  | 'Container'
  | 'Pallet'
  | 'Order'
  | 'Transfer'
  | 'Receipt'
  | 'Shipment'
  | 'User'
  | 'Equipment'
  | 'Other';

export type ReasonCodeType =
  | 'Adjustment'
  | 'Return'
  | 'Damage'
  | 'Expiry'
  | 'QualityHold'
  | 'Transfer'
  | 'Scrap'
  | 'Found'
  | 'Lost'
  | 'Other';

export type ResetFrequency = 'Never' | 'Daily' | 'Weekly' | 'Monthly' | 'Yearly';

export type SettingValueType = 'String' | 'Number' | 'Boolean' | 'Json' | 'DateTime';

// ============================================================================
// Barcode Prefixes
// ============================================================================

export interface BarcodePrefixDto {
  id: string;
  prefixType: BarcodePrefixType;
  prefix: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateBarcodePrefixRequest {
  prefixType: BarcodePrefixType;
  prefix: string;
  description?: string;
  isActive?: boolean;
}

export interface UpdateBarcodePrefixRequest {
  prefix?: string;
  description?: string;
  isActive?: boolean;
}

// ============================================================================
// Reason Codes
// ============================================================================

export interface ReasonCodeDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  reasonType: ReasonCodeType;
  isActive: boolean;
  requiresNotes: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateReasonCodeRequest {
  code: string;
  name: string;
  description?: string;
  reasonType: ReasonCodeType;
  isActive?: boolean;
  requiresNotes?: boolean;
  sortOrder?: number;
}

export interface UpdateReasonCodeRequest {
  code?: string;
  name?: string;
  description?: string;
  isActive?: boolean;
  requiresNotes?: boolean;
  sortOrder?: number;
}

// ============================================================================
// Number Sequences
// ============================================================================

export interface NumberSequenceDto {
  id: string;
  sequenceName: string;
  prefix: string;
  suffix?: string;
  currentNumber: number;
  incrementBy: number;
  minDigits: number;
  resetFrequency: ResetFrequency;
  lastResetDate?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateNumberSequenceRequest {
  sequenceName: string;
  prefix: string;
  suffix?: string;
  startNumber?: number;
  incrementBy?: number;
  minDigits?: number;
  resetFrequency?: ResetFrequency;
  isActive?: boolean;
}

export interface UpdateNumberSequenceRequest {
  prefix?: string;
  suffix?: string;
  incrementBy?: number;
  minDigits?: number;
  resetFrequency?: ResetFrequency;
  isActive?: boolean;
}

// ============================================================================
// System Settings
// ============================================================================

export interface SystemSettingDto {
  id: string;
  category: string;
  key: string;
  value: string;
  valueType: SettingValueType;
  description?: string;
  isEditable: boolean;
  isVisible: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateSystemSettingRequest {
  value: string;
}

// ============================================================================
// Query Params
// ============================================================================

export interface BarcodePrefixFilters {
  prefixType?: BarcodePrefixType;
  isActive?: boolean;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface ReasonCodeFilters {
  reasonType?: ReasonCodeType;
  isActive?: boolean;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface NumberSequenceFilters {
  isActive?: boolean;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface SystemSettingFilters {
  category?: string;
  isVisible?: boolean;
  searchTerm?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type BarcodePrefixResponse = ApiResponse<BarcodePrefixDto>;
export type BarcodePrefixListResponse = ApiResponse<PaginatedResponse<BarcodePrefixDto>>;

export type ReasonCodeResponse = ApiResponse<ReasonCodeDto>;
export type ReasonCodeListResponse = ApiResponse<PaginatedResponse<ReasonCodeDto>>;

export type NumberSequenceResponse = ApiResponse<NumberSequenceDto>;
export type NumberSequenceListResponse = ApiResponse<PaginatedResponse<NumberSequenceDto>>;

export type SystemSettingResponse = ApiResponse<SystemSettingDto>;
export type SystemSettingListResponse = ApiResponse<SystemSettingDto[]>;
