/**
 * Orders API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Orders/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type SalesOrderStatus =
  | 'Draft'
  | 'Pending'
  | 'Confirmed'
  | 'Allocated'
  | 'PartiallyAllocated'
  | 'Picking'
  | 'PartiallyPicked'
  | 'Picked'
  | 'Packing'
  | 'Packed'
  | 'Shipped'
  | 'PartiallyShipped'
  | 'Delivered'
  | 'Cancelled'
  | 'OnHold';

export type PurchaseOrderStatus =
  | 'Draft'
  | 'Pending'
  | 'Confirmed'
  | 'PartiallyReceived'
  | 'Received'
  | 'Completed'
  | 'Cancelled'
  | 'OnHold';

export type OrderPriority = 'Low' | 'Normal' | 'High' | 'Urgent';

// ============================================================================
// Sales Order Types
// ============================================================================

export interface SalesOrderDto {
  id: string;
  orderNumber: string;
  externalReference?: string;

  // Customer
  customerId: string;
  customerCode?: string;
  customerName?: string;

  // Warehouse
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;

  // Status
  status: SalesOrderStatus;
  priority: OrderPriority;

  // Dates
  orderDate: string;
  requiredDate?: string;
  shippedDate?: string;

  // Shipping address
  shipToName?: string;
  shipToAddressLine1?: string;
  shipToAddressLine2?: string;
  shipToCity?: string;
  shipToRegion?: string;
  shipToPostalCode?: string;
  shipToCountryCode?: string;

  // Shipping info
  carrierCode?: string;
  serviceLevel?: string;
  shippingInstructions?: string;

  // Totals
  totalLines: number;
  totalQuantity: number;
  allocatedQuantity: number;
  pickedQuantity: number;
  shippedQuantity: number;

  // Notes
  notes?: string;
  internalNotes?: string;

  createdAt: string;
  updatedAt?: string;

  // Lines
  lines?: SalesOrderLineDto[];
}

export interface SalesOrderLineDto {
  id: string;
  orderId: string;
  lineNumber: number;

  // Product
  productId: string;
  sku: string;
  productName?: string;

  // Quantities
  quantityOrdered: number;
  quantityAllocated: number;
  quantityPicked: number;
  quantityShipped: number;
  quantityCancelled: number;
  quantityOutstanding: number;

  // Batch/Serial
  requiredBatchNumber?: string;
  requiredExpiryDate?: string;

  notes?: string;
}

export interface CreateSalesOrderRequest {
  orderNumber?: string;
  externalReference?: string;
  customerId: string;
  warehouseId: string;
  priority?: OrderPriority;
  requiredDate?: string;

  // Shipping address
  shipToName?: string;
  shipToAddressLine1?: string;
  shipToAddressLine2?: string;
  shipToCity?: string;
  shipToRegion?: string;
  shipToPostalCode?: string;
  shipToCountryCode?: string;

  // Shipping info
  carrierCode?: string;
  serviceLevel?: string;
  shippingInstructions?: string;

  notes?: string;
  internalNotes?: string;

  lines?: CreateSalesOrderLineRequest[];
}

export interface CreateSalesOrderLineRequest {
  productId: string;
  quantityOrdered: number;
  requiredBatchNumber?: string;
  requiredExpiryDate?: string;
  notes?: string;
}

export interface UpdateSalesOrderRequest {
  externalReference?: string;
  customerId?: string;
  warehouseId?: string;
  priority?: OrderPriority;
  requiredDate?: string;

  shipToName?: string;
  shipToAddressLine1?: string;
  shipToAddressLine2?: string;
  shipToCity?: string;
  shipToRegion?: string;
  shipToPostalCode?: string;
  shipToCountryCode?: string;

  carrierCode?: string;
  serviceLevel?: string;
  shippingInstructions?: string;

  notes?: string;
  internalNotes?: string;
}

export interface UpdateSalesOrderStatusRequest {
  status: SalesOrderStatus;
  notes?: string;
}

export interface AddSalesOrderLineRequest {
  productId: string;
  quantityOrdered: number;
  requiredBatchNumber?: string;
  requiredExpiryDate?: string;
  notes?: string;
}

export interface UpdateSalesOrderLineRequest {
  quantityOrdered?: number;
  requiredBatchNumber?: string;
  requiredExpiryDate?: string;
  notes?: string;
}

export interface SalesOrderFilters extends PaginationParams {
  search?: string;
  status?: SalesOrderStatus;
  priority?: OrderPriority;
  customerId?: string;
  warehouseId?: string;
  orderDateFrom?: string;
  orderDateTo?: string;
  requiredDateFrom?: string;
  requiredDateTo?: string;
}

// ============================================================================
// Purchase Order Types
// ============================================================================

export interface PurchaseOrderDto {
  id: string;
  orderNumber: string;
  externalReference?: string;

  // Supplier
  supplierId: string;
  supplierCode?: string;
  supplierName?: string;

  // Warehouse
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;

  // Status
  status: PurchaseOrderStatus;

  // Dates
  orderDate: string;
  expectedDate?: string;
  receivedDate?: string;

  // Receiving dock
  receivingDockId?: string;
  receivingDockCode?: string;

  // Totals
  totalLines: number;
  totalQuantity: number;
  receivedQuantity: number;

  // Notes
  notes?: string;
  internalNotes?: string;

  createdAt: string;
  updatedAt?: string;

  // Lines
  lines?: PurchaseOrderLineDto[];
}

export interface PurchaseOrderLineDto {
  id: string;
  orderId: string;
  lineNumber: number;

  // Product
  productId: string;
  sku: string;
  productName?: string;

  // Quantities
  quantityOrdered: number;
  quantityReceived: number;
  quantityCancelled: number;
  quantityOutstanding: number;

  // Expected batch/serial
  expectedBatchNumber?: string;
  expectedExpiryDate?: string;

  notes?: string;
}

export interface CreatePurchaseOrderRequest {
  orderNumber?: string;
  externalReference?: string;
  supplierId: string;
  warehouseId: string;
  expectedDate?: string;
  receivingDockId?: string;
  notes?: string;
  internalNotes?: string;
  lines?: CreatePurchaseOrderLineRequest[];
}

export interface CreatePurchaseOrderLineRequest {
  productId: string;
  quantityOrdered: number;
  expectedBatchNumber?: string;
  expectedExpiryDate?: string;
  notes?: string;
}

export interface UpdatePurchaseOrderRequest {
  externalReference?: string;
  supplierId?: string;
  warehouseId?: string;
  expectedDate?: string;
  receivingDockId?: string;
  notes?: string;
  internalNotes?: string;
}

export interface UpdatePurchaseOrderStatusRequest {
  status: PurchaseOrderStatus;
  notes?: string;
}

export interface AddPurchaseOrderLineRequest {
  productId: string;
  quantityOrdered: number;
  expectedBatchNumber?: string;
  expectedExpiryDate?: string;
  notes?: string;
}

export interface UpdatePurchaseOrderLineRequest {
  quantityOrdered?: number;
  expectedBatchNumber?: string;
  expectedExpiryDate?: string;
  notes?: string;
}

export interface ReceivePurchaseOrderLineRequest {
  quantityReceived: number;
  batchNumber?: string;
  expiryDate?: string;
  locationId?: string;
  notes?: string;
}

export interface PurchaseOrderFilters extends PaginationParams {
  search?: string;
  status?: PurchaseOrderStatus;
  supplierId?: string;
  warehouseId?: string;
  orderDateFrom?: string;
  orderDateTo?: string;
  expectedDateFrom?: string;
  expectedDateTo?: string;
}

// ============================================================================
// Customer Types
// ============================================================================

export interface CustomerDto {
  id: string;
  code: string;
  name: string;
  contactName?: string;
  email?: string;
  phone?: string;

  // Address
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;

  // Business info
  taxId?: string;
  paymentTerms?: string;

  isActive: boolean;
  createdAt: string;
  updatedAt?: string;

  orderCount: number;
}

export interface CreateCustomerRequest {
  code: string;
  name: string;
  contactName?: string;
  email?: string;
  phone?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  taxId?: string;
  paymentTerms?: string;
  isActive?: boolean;
}

export interface UpdateCustomerRequest {
  name?: string;
  contactName?: string;
  email?: string;
  phone?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  taxId?: string;
  paymentTerms?: string;
  isActive?: boolean;
}

export interface CustomerFilters extends PaginationParams {
  search?: string;
  isActive?: boolean;
  countryCode?: string;
}

// ============================================================================
// Supplier Types
// ============================================================================

export interface SupplierDto {
  id: string;
  code: string;
  name: string;
  contactName?: string;
  email?: string;
  phone?: string;

  // Address
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;

  // Business info
  taxId?: string;
  paymentTerms?: string;
  leadTimeDays?: number;

  isActive: boolean;
  createdAt: string;
  updatedAt?: string;

  orderCount: number;
}

export interface CreateSupplierRequest {
  code: string;
  name: string;
  contactName?: string;
  email?: string;
  phone?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  taxId?: string;
  paymentTerms?: string;
  leadTimeDays?: number;
  isActive?: boolean;
}

export interface UpdateSupplierRequest {
  name?: string;
  contactName?: string;
  email?: string;
  phone?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  region?: string;
  postalCode?: string;
  countryCode?: string;
  taxId?: string;
  paymentTerms?: string;
  leadTimeDays?: number;
  isActive?: boolean;
}

export interface SupplierFilters extends PaginationParams {
  search?: string;
  isActive?: boolean;
  countryCode?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type SalesOrderResponse = ApiResponse<SalesOrderDto>;
export type SalesOrderListResponse = ApiResponse<PaginatedResponse<SalesOrderDto>>;
export type SalesOrderLineResponse = ApiResponse<SalesOrderLineDto>;

export type PurchaseOrderResponse = ApiResponse<PurchaseOrderDto>;
export type PurchaseOrderListResponse = ApiResponse<PaginatedResponse<PurchaseOrderDto>>;
export type PurchaseOrderLineResponse = ApiResponse<PurchaseOrderLineDto>;

export type CustomerResponse = ApiResponse<CustomerDto>;
export type CustomerListResponse = ApiResponse<PaginatedResponse<CustomerDto>>;

export type SupplierResponse = ApiResponse<SupplierDto>;
export type SupplierListResponse = ApiResponse<PaginatedResponse<SupplierDto>>;
