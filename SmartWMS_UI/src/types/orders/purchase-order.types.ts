import { Entity, UserRef } from '@/models';

/**
 * Purchase order (inbound) record
 */
export interface PurchaseOrder extends Entity {
  orderNumber: string;
  externalReference?: string;
  vendorPO?: string;

  supplierId: string;
  supplierName: string;
  supplierCode?: string;

  warehouseId: string;
  warehouseCode: string;

  status: PurchaseOrderStatus;

  orderDate: string;
  expectedDate?: string;
  receivedDate?: string;

  totalLines: number;
  totalQuantity: number;
  receivedQuantity: number;

  currencyCode: string;
  totalAmount?: number;

  notes?: string;
  createdBy?: UserRef;
}

/**
 * Purchase order status workflow
 */
export enum PurchaseOrderStatus {
  Draft = 'DRAFT',
  Pending = 'PENDING',
  Confirmed = 'CONFIRMED',
  PartiallyReceived = 'PARTIALLY_RECEIVED',
  Received = 'RECEIVED',
  Closed = 'CLOSED',
  Cancelled = 'CANCELLED',
}

/**
 * Purchase order line item
 */
export interface PurchaseOrderLine extends Entity {
  orderId: string;
  orderNumber: string;
  lineNumber: number;

  sku: string;
  productName: string;
  description?: string;
  supplierSku?: string;

  quantityOrdered: number;
  quantityReceived: number;
  quantityRejected: number;

  lineStatus: PurchaseLineStatus;

  unitCost?: number;
  lineTotal?: number;

  expectedDate?: string;
  receivedDate?: string;

  toWarehouseId?: string;
  toLocationId?: string;

  batchNumber?: string;
  expiryDate?: string;

  notes?: string;
}

/**
 * Purchase line status
 */
export enum PurchaseLineStatus {
  Pending = 'PENDING',
  PartiallyReceived = 'PARTIALLY_RECEIVED',
  Received = 'RECEIVED',
  OverReceived = 'OVER_RECEIVED',
  Cancelled = 'CANCELLED',
}

/**
 * Purchase order filter parameters
 */
export interface PurchaseOrderFilter {
  status?: PurchaseOrderStatus | PurchaseOrderStatus[];
  supplierId?: string;
  warehouseId?: string;
  orderDateFrom?: string;
  orderDateTo?: string;
  expectedDateFrom?: string;
  expectedDateTo?: string;
  search?: string;
}

/**
 * Receiving input for PO line
 */
export interface ReceivingInput {
  lineId: string;
  quantityReceived: number;
  quantityRejected?: number;
  toLocationId: string;
  batchNumber?: string;
  expiryDate?: string;
  serialNumbers?: string[];
  notes?: string;
}

/**
 * Purchase order creation payload
 */
export interface PurchaseOrderInput {
  supplierId: string;
  warehouseId: string;
  expectedDate?: string;
  externalReference?: string;
  currencyCode?: string;
  notes?: string;
  lines: PurchaseOrderLineInput[];
}

/**
 * Purchase order line input
 */
export interface PurchaseOrderLineInput {
  sku: string;
  quantity: number;
  unitCost?: number;
  expectedDate?: string;
  toLocationId?: string;
}
