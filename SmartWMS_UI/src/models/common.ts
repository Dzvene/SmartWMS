/**
 * Core domain models for SmartWMS
 * Standard warehouse management data structures
 */

/**
 * Base entity with standard tracking fields
 */
export interface Entity {
  id: string | number;
  createdAt?: string;
  modifiedAt?: string;
}

/**
 * Reference item with identifier and display name
 */
export interface ReferenceItem<TKey = number> {
  id: TKey;
  name: string;
}

/**
 * Lookup entry with code and description
 * Common pattern for master data (carriers, payment terms, etc.)
 */
export interface LookupEntry {
  code: string;
  description: string;
}

/**
 * Generic attribute pair for dynamic properties
 */
export interface Attribute<T = string> {
  key: string;
  value: T;
}

/**
 * Physical dimensions for storage calculations
 */
export interface Dimensions {
  widthMm: number;
  heightMm: number;
  depthMm: number;
}

/**
 * Weight specification
 */
export interface Weight {
  grossKg: number;
  netKg?: number;
}

/**
 * Address structure for shipping/receiving
 */
export interface PostalAddress {
  addressLine1: string;
  addressLine2?: string;
  addressLine3?: string;
  city: string;
  postalCode: string;
  countryCode: string;
  region?: string;
}

/**
 * Sorting configuration
 */
export enum SortDirection {
  Ascending = 'asc',
  Descending = 'desc',
}

/**
 * Pagination request parameters
 */
export interface PageRequest {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: SortDirection;
}

/**
 * Paginated response wrapper
 */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * API error response
 */
export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, string[]>;
}

/**
 * Form field validation error
 */
export interface FieldError {
  field: string;
  message: string;
  params?: Record<string, unknown>;
}

/**
 * File upload result
 */
export interface UploadResult {
  fileId: string;
  originalName: string;
  size: number;
  mimeType: string;
}

/**
 * UI display size variants
 */
export type ComponentSize = 'small' | 'medium' | 'large';

/**
 * Date range for filtering
 */
export interface DateRange {
  from: Date | string;
  to: Date | string;
}

/**
 * User reference for audit trails
 */
export interface UserRef {
  userId: number;
  username: string;
  displayName?: string;
}
