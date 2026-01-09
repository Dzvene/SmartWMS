/**
 * Products API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Inventory/DTOs/
 */

import type { ApiResponse, PaginatedResponse } from '../../types';

// ============================================================================
// Product Types
// ============================================================================

export interface ProductResponse {
  id: string;
  sku: string;
  name: string;
  description?: string;

  // Category
  categoryId?: string;
  categoryName?: string;
  categoryPath?: string;

  // Identification
  barcode?: string;
  alternativeBarcodes?: string;

  // Dimensions (in millimeters)
  widthMm?: number;
  heightMm?: number;
  depthMm?: number;

  // Weight (in kilograms)
  grossWeightKg?: number;
  netWeightKg?: number;

  // Unit of measure
  unitOfMeasure: string;
  unitsPerCase?: number;
  casesPerPallet?: number;

  // Status
  isActive: boolean;

  // Tracking requirements
  isBatchTracked: boolean;
  isSerialTracked: boolean;
  hasExpiryDate: boolean;

  // Inventory levels for reordering
  minStockLevel?: number;
  maxStockLevel?: number;
  reorderPoint?: number;

  // Supplier
  defaultSupplierId?: string;
  defaultSupplierName?: string;

  // Media
  imageUrl?: string;

  // Calculated
  stockLevelCount: number;
  totalOnHand?: number;

  // Audit
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string;
  categoryId?: string;
  barcode?: string;
  alternativeBarcodes?: string;
  widthMm?: number;
  heightMm?: number;
  depthMm?: number;
  grossWeightKg?: number;
  netWeightKg?: number;
  unitOfMeasure: string;
  unitsPerCase?: number;
  casesPerPallet?: number;
  isActive?: boolean;
  isBatchTracked?: boolean;
  isSerialTracked?: boolean;
  hasExpiryDate?: boolean;
  minStockLevel?: number;
  maxStockLevel?: number;
  reorderPoint?: number;
  defaultSupplierId?: string;
  imageUrl?: string;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  categoryId?: string;
  barcode?: string;
  alternativeBarcodes?: string;
  widthMm?: number;
  heightMm?: number;
  depthMm?: number;
  grossWeightKg?: number;
  netWeightKg?: number;
  unitOfMeasure?: string;
  unitsPerCase?: number;
  casesPerPallet?: number;
  isActive?: boolean;
  isBatchTracked?: boolean;
  isSerialTracked?: boolean;
  hasExpiryDate?: boolean;
  minStockLevel?: number;
  maxStockLevel?: number;
  reorderPoint?: number;
  defaultSupplierId?: string;
  imageUrl?: string;
}

export interface ProductsListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  isActive?: boolean;
}

// ============================================================================
// Product Category Types
// ============================================================================

export interface ProductCategoryResponse {
  id: string;
  code: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  level: number;
  path?: string;
  isActive: boolean;

  // Product Defaults - applied when creating products in this category
  defaultUnitOfMeasure?: string;
  defaultStorageZoneType?: string;

  // Tracking Requirements - enforced for all products in this category
  requiresBatchTracking: boolean;
  requiresSerialTracking: boolean;
  requiresExpiryDate: boolean;

  // Handling & Storage
  handlingInstructions?: string;
  minTemperature?: number;
  maxTemperature?: number;
  isHazardous: boolean;
  isFragile: boolean;

  // Counts
  productCount: number;
  childCategoryCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface ProductCategoryTreeResponse {
  id: string;
  code: string;
  name: string;
  level: number;
  productCount: number;
  isActive: boolean;
  requiresBatchTracking: boolean;
  requiresSerialTracking: boolean;
  requiresExpiryDate: boolean;
  children: ProductCategoryTreeResponse[];
}

export interface CreateProductCategoryRequest {
  code: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
  isActive?: boolean;

  // Product Defaults
  defaultUnitOfMeasure?: string;
  defaultStorageZoneType?: string;

  // Tracking Requirements
  requiresBatchTracking?: boolean;
  requiresSerialTracking?: boolean;
  requiresExpiryDate?: boolean;

  // Handling & Storage
  handlingInstructions?: string;
  minTemperature?: number;
  maxTemperature?: number;
  isHazardous?: boolean;
  isFragile?: boolean;
}

export interface UpdateProductCategoryRequest {
  code?: string;
  name?: string;
  description?: string;
  parentCategoryId?: string;
  isActive?: boolean;

  // Product Defaults
  defaultUnitOfMeasure?: string;
  defaultStorageZoneType?: string;

  // Tracking Requirements
  requiresBatchTracking?: boolean;
  requiresSerialTracking?: boolean;
  requiresExpiryDate?: boolean;

  // Handling & Storage
  handlingInstructions?: string;
  minTemperature?: number;
  maxTemperature?: number;
  isHazardous?: boolean;
  isFragile?: boolean;
}

export interface CategoriesListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  parentCategoryId?: string;
  isActive?: boolean;
}

// ============================================================================
// Response Types
// ============================================================================

export type ProductsListResponse = ApiResponse<PaginatedResponse<ProductResponse>>;
export type ProductDetailResponse = ApiResponse<ProductResponse>;
export type CategoriesListResponse = ApiResponse<PaginatedResponse<ProductCategoryResponse>>;
export type CategoryDetailResponse = ApiResponse<ProductCategoryResponse>;
export type CategoryTreeResponse = ApiResponse<ProductCategoryTreeResponse[]>;
