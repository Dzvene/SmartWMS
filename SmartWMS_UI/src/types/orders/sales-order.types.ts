import { Entity, PostalAddress, UserRef } from '@/models';

/**
 * Sales order (outbound) record
 */
export interface SalesOrder extends Entity {
  orderNumber: string;
  externalReference?: string;
  customerPO?: string;

  customerId: string;
  customerName: string;
  customerCode?: string;

  warehouseId: string;
  warehouseCode: string;

  status: SalesOrderStatus;
  priority: OrderPriority;

  orderDate: string;
  requiredDate?: string;
  promisedDate?: string;
  shippedDate?: string;

  shippingAddress: PostalAddress;
  billingAddress?: PostalAddress;

  carrierId?: string;
  carrierName?: string;
  serviceType?: string;
  trackingNumber?: string;

  totalLines: number;
  totalQuantity: number;
  pickedQuantity: number;
  shippedQuantity: number;

  currencyCode: string;
  totalAmount?: number;

  notes?: string;
  tags?: string[];

  createdBy?: UserRef;
  lockedBy?: UserRef;
  lockedAt?: string;
  lockReason?: string;
}

/**
 * Sales order status workflow
 */
export enum SalesOrderStatus {
  Draft = 'DRAFT',
  Pending = 'PENDING',
  Confirmed = 'CONFIRMED',
  Allocated = 'ALLOCATED',
  PartiallyPicked = 'PARTIALLY_PICKED',
  Picked = 'PICKED',
  Packed = 'PACKED',
  Shipped = 'SHIPPED',
  Delivered = 'DELIVERED',
  Cancelled = 'CANCELLED',
  OnHold = 'ON_HOLD',
}

/**
 * Order priority levels
 */
export enum OrderPriority {
  Low = 'LOW',
  Normal = 'NORMAL',
  High = 'HIGH',
  Urgent = 'URGENT',
}

/**
 * Sales order line item
 */
export interface SalesOrderLine extends Entity {
  orderId: string;
  orderNumber: string;
  lineNumber: number;

  sku: string;
  productName: string;
  description?: string;

  quantityOrdered: number;
  quantityAllocated: number;
  quantityPicked: number;
  quantityShipped: number;

  lineStatus: OrderLineStatus;

  unitPrice?: number;
  lineTotal?: number;

  requestedDate?: string;
  promisedDate?: string;

  warehouseId?: string;
  fromLocationId?: string;

  batchNumber?: string;
  serialNumbers?: string[];

  notes?: string;
}

/**
 * Order line status
 */
export enum OrderLineStatus {
  Pending = 'PENDING',
  Available = 'AVAILABLE',
  PartiallyAvailable = 'PARTIALLY_AVAILABLE',
  NotAvailable = 'NOT_AVAILABLE',
  Allocated = 'ALLOCATED',
  Picking = 'PICKING',
  Picked = 'PICKED',
  Packed = 'PACKED',
  Shipped = 'SHIPPED',
  Cancelled = 'CANCELLED',
  Backordered = 'BACKORDERED',
}

/**
 * Sales order filter parameters
 */
export interface SalesOrderFilter {
  status?: SalesOrderStatus | SalesOrderStatus[];
  customerId?: string;
  warehouseId?: string;
  carrierId?: string;
  priority?: OrderPriority;
  orderDateFrom?: string;
  orderDateTo?: string;
  requiredDateFrom?: string;
  requiredDateTo?: string;
  search?: string;
  isLocked?: boolean;
}

/**
 * Order hold/lock request
 */
export interface OrderHoldRequest {
  orderId: string;
  reason: string;
  notes?: string;
}

/**
 * Allocation summary for order line
 */
export interface LineAllocation {
  lineId: string;
  locationId: string;
  locationCode: string;
  quantity: number;
  batchNumber?: string;
  expiryDate?: string;
}
