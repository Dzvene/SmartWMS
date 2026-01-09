import { Entity, Dimensions, Weight } from '@/models';

/**
 * SKU (Stock Keeping Unit) - Core product definition
 */
export interface Product extends Entity {
  sku: string;
  name: string;
  description?: string;
  category?: string;
  barcode?: string;
  alternativeBarcodes?: string[];

  dimensions?: Dimensions;
  weight?: Weight;

  unitOfMeasure: string;
  unitsPerCase?: number;
  casesPerPallet?: number;

  isActive: boolean;
  isBatchTracked: boolean;
  isSerialTracked: boolean;
  hasExpiryDate: boolean;

  minStockLevel?: number;
  maxStockLevel?: number;
  reorderPoint?: number;

  supplierId?: number;
  supplierSku?: string;

  imageUrl?: string;
  customAttributes?: Record<string, string>;
}

/**
 * Product search/filter parameters
 */
export interface ProductFilter {
  search?: string;
  category?: string;
  supplierId?: number;
  isActive?: boolean;
  hasStock?: boolean;
}

/**
 * Product creation/update payload
 */
export interface ProductInput {
  sku: string;
  name: string;
  description?: string;
  category?: string;
  barcode?: string;
  unitOfMeasure: string;
  isBatchTracked?: boolean;
  isSerialTracked?: boolean;
  hasExpiryDate?: boolean;
  dimensions?: Dimensions;
  weight?: Weight;
}

/**
 * Product category for grouping
 */
export interface ProductCategory {
  id: number;
  name: string;
  parentId?: number;
  productCount: number;
}

/**
 * Product search result with stock info
 */
export interface ProductSearchResult extends Product {
  totalStock: number;
  availableStock: number;
  reservedStock: number;
}
