/**
 * Reports API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Reports/DTOs/
 */

import type { ApiResponse } from '../../types';

// ============================================================================
// Inventory Summary Report
// ============================================================================

export interface InventorySummaryReport {
  generatedAt: string;
  warehouseId?: string;
  warehouseName?: string;

  // Overall metrics
  totalProducts: number;
  productsWithStock: number;
  productsOutOfStock: number;
  productsLowStock: number;

  totalQuantityOnHand: number;
  totalQuantityReserved: number;
  totalQuantityAvailable: number;

  // Location metrics
  totalLocations: number;
  occupiedLocations: number;
  emptyLocations: number;
  locationUtilizationPercent: number;

  // Top items
  topProductsByQuantity: ProductQuantityItem[];
  lowStockProducts: ProductQuantityItem[];
  expiringStock: ExpiringStockItem[];
}

// ============================================================================
// Stock Movement Report
// ============================================================================

export interface StockMovementReport {
  generatedAt: string;
  dateFrom: string;
  dateTo: string;
  warehouseId?: string;
  warehouseName?: string;

  // Summary by movement type
  movementsByType: MovementTypeSummary[];

  // Daily breakdown
  dailyMovements: DailyMovementSummary[];

  // Top movers
  topMovedProducts: ProductMovementItem[];
}

// ============================================================================
// Order Fulfillment Report
// ============================================================================

export interface OrderFulfillmentReport {
  generatedAt: string;
  dateFrom: string;
  dateTo: string;

  // Sales orders metrics
  totalSalesOrders: number;
  ordersDelivered: number;
  ordersInProgress: number;
  ordersPending: number;
  ordersCancelled: number;
  fulfillmentRatePercent: number;

  // Picking metrics
  totalPickTasks: number;
  pickTasksCompleted: number;
  pickTasksPending: number;
  pickCompletionRatePercent: number;

  // Shipment metrics
  totalShipments: number;
  shipmentsDelivered: number;
  shipmentsInTransit: number;

  // Daily breakdown
  dailyOrders: DailyOrderSummary[];
}

// ============================================================================
// Receiving Report
// ============================================================================

export interface ReceivingReport {
  generatedAt: string;
  dateFrom: string;
  dateTo: string;

  // Purchase orders metrics
  totalPurchaseOrders: number;
  pOsReceived: number;
  pOsPartiallyReceived: number;
  pOsPending: number;

  // Goods receipts metrics
  totalGoodsReceipts: number;
  receiptsCompleted: number;
  receiptsInProgress: number;
  totalQuantityReceived: number;

  // Quality metrics
  quantityGood: number;
  quantityDamaged: number;
  quantityQuarantine: number;
  qualityPassRatePercent: number;

  // Daily breakdown
  dailyReceiving: DailyReceivingSummary[];
}

// ============================================================================
// Warehouse Utilization Report
// ============================================================================

export interface WarehouseUtilizationReport {
  generatedAt: string;
  warehouseId: string;
  warehouseName?: string;

  // Zone breakdown
  zoneUtilizations: ZoneUtilization[];

  // Overall metrics
  totalLocations: number;
  occupiedLocations: number;
  emptyLocations: number;
  overallUtilizationPercent: number;

  // Capacity
  estimatedCapacityUsedPercent: number;
}

// ============================================================================
// Helper Types
// ============================================================================

export interface ProductQuantityItem {
  productId: string;
  sku?: string;
  productName?: string;
  quantityOnHand: number;
  minStockLevel?: number;
}

export interface ExpiringStockItem {
  productId: string;
  sku?: string;
  productName?: string;
  batchNumber?: string;
  expiryDate?: string;
  quantity: number;
  daysUntilExpiry: number;
}

export interface MovementTypeSummary {
  movementType: string;
  count: number;
  totalQuantity: number;
}

export interface DailyMovementSummary {
  date: string;
  receiptCount: number;
  issueCount: number;
  transferCount: number;
  adjustmentCount: number;
  totalQuantityMoved: number;
}

export interface ProductMovementItem {
  productId: string;
  sku?: string;
  productName?: string;
  movementCount: number;
  totalQuantityMoved: number;
}

export interface DailyOrderSummary {
  date: string;
  ordersCreated: number;
  ordersShipped: number;
  ordersDelivered: number;
  linesProcessed: number;
}

export interface DailyReceivingSummary {
  date: string;
  receiptsCreated: number;
  receiptsCompleted: number;
  quantityReceived: number;
}

export interface ZoneUtilization {
  zoneId: string;
  zoneName?: string;
  zoneType?: string;
  totalLocations: number;
  occupiedLocations: number;
  utilizationPercent: number;
}

// ============================================================================
// Query Params
// ============================================================================

export interface ReportDateRangeParams {
  dateFrom?: string;
  dateTo?: string;
  warehouseId?: string;
}

export interface InventorySummaryParams {
  warehouseId?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type InventorySummaryResponse = ApiResponse<InventorySummaryReport>;
export type StockMovementResponse = ApiResponse<StockMovementReport>;
export type OrderFulfillmentResponse = ApiResponse<OrderFulfillmentReport>;
export type ReceivingReportResponse = ApiResponse<ReceivingReport>;
export type WarehouseUtilizationResponse = ApiResponse<WarehouseUtilizationReport>;
