/**
 * Dashboard API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Dashboard/DTOs/
 */

import type { ApiResponse } from '../../types';

// ============================================================================
// Overview Types
// ============================================================================

export interface DashboardOverviewDto {
  orders: OrdersOverviewDto;
  inventory: InventoryOverviewDto;
  fulfillment: FulfillmentOverviewDto;
  warehouse: WarehouseOverviewDto;
}

export interface OrdersOverviewDto {
  totalSalesOrders: number;
  pendingSalesOrders: number;
  totalPurchaseOrders: number;
  pendingPurchaseOrders: number;
  ordersToday: number;
  ordersThisWeek: number;
  totalOrderValue: number;
}

export interface InventoryOverviewDto {
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  totalLocations: number;
  occupiedLocations: number;
  totalStockValue: number;
}

export interface FulfillmentOverviewDto {
  pendingPickTasks: number;
  inProgressPickTasks: number;
  completedToday: number;
  pendingPackTasks: number;
  shipmentsPending: number;
  shipmentsToday: number;
  pickAccuracyRate: number;
}

export interface WarehouseOverviewDto {
  totalWarehouses: number;
  totalZones: number;
  totalLocations: number;
  locationUtilization: number;
  activeEquipment: number;
  maintenanceNeeded: number;
}

// ============================================================================
// KPI Types
// ============================================================================

export interface KpiMetricsDto {
  metrics: KpiMetricDto[];
  period: string;
  periodType: string;
}

export interface KpiMetricDto {
  code: string;
  name: string;
  category: string;
  value: number;
  target?: number;
  previousValue?: number;
  unit: string;
  trend?: string;
  changePercent?: number;
}

export interface KpiTrendDto {
  code: string;
  name: string;
  dataPoints: KpiDataPointDto[];
  target?: number;
}

export interface KpiDataPointDto {
  date: string;
  value: number;
}

// ============================================================================
// Activity Feed Types
// ============================================================================

export interface ActivityFeedDto {
  items: ActivityItemDto[];
  totalCount: number;
  lastUpdated?: string;
}

export interface ActivityItemDto {
  id: string;
  type: string;
  title: string;
  description?: string;
  entityType?: string;
  entityId?: string;
  userName?: string;
  createdAt: string;
  icon?: string;
  color?: string;
}

// ============================================================================
// Chart Types
// ============================================================================

export interface ChartDataDto {
  chartType: string;
  title: string;
  labels: string[];
  series: ChartSeriesDto[];
}

export interface ChartSeriesDto {
  name: string;
  data: number[];
  color?: string;
}

export interface OrdersTrendDto {
  labels: string[];
  salesOrders: number[];
  purchaseOrders: number[];
  shipments: number[];
}

export interface InventoryTrendDto {
  labels: string[];
  stockLevels: number[];
  movements: number[];
  adjustments: number[];
}

export interface FulfillmentTrendDto {
  labels: string[];
  pickTasks: number[];
  packTasks: number[];
  shipments: number[];
}

// ============================================================================
// Alerts & Tasks Types
// ============================================================================

export interface DashboardAlertsDto {
  critical: AlertItemDto[];
  warning: AlertItemDto[];
  info: AlertItemDto[];
  totalCount: number;
}

export interface AlertItemDto {
  type: string;
  severity: string;
  title: string;
  description?: string;
  actionUrl?: string;
  createdAt: string;
}

export interface PendingTasksDto {
  pickTasks: TaskSummaryDto[];
  putawayTasks: TaskSummaryDto[];
  packTasks: TaskSummaryDto[];
  cycleCountTasks: TaskSummaryDto[];
  totalPending: number;
}

export interface TaskSummaryDto {
  id: string;
  type: string;
  status: string;
  priority?: string;
  assignedTo?: string;
  dueDate?: string;
  location?: string;
}

// ============================================================================
// Quick Stats Types
// ============================================================================

export interface QuickStatsDto {
  ordersToShip: number;
  lowStockItems: number;
  pendingReceipts: number;
  openTasks: number;
  activeUsers: number;
  todayRevenue: number;
}

export interface WarehouseStatsDto {
  warehouseId: string;
  warehouseName: string;
  totalLocations: number;
  usedLocations: number;
  utilization: number;
  pendingTasks: number;
  activeWorkers: number;
}

// ============================================================================
// Query Params
// ============================================================================

export interface DashboardQueryParams {
  warehouseId?: string;
  fromDate?: string;
  toDate?: string;
  period?: string;
}

export interface TrendQueryParams {
  days?: number;
  granularity?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type DashboardOverviewResponse = ApiResponse<DashboardOverviewDto>;
export type QuickStatsResponse = ApiResponse<QuickStatsDto>;
export type KpiMetricsResponse = ApiResponse<KpiMetricsDto>;
export type KpiTrendResponse = ApiResponse<KpiTrendDto>;
export type OrdersTrendResponse = ApiResponse<OrdersTrendDto>;
export type InventoryTrendResponse = ApiResponse<InventoryTrendDto>;
export type FulfillmentTrendResponse = ApiResponse<FulfillmentTrendDto>;
export type ActivityFeedResponse = ApiResponse<ActivityFeedDto>;
export type AlertsResponse = ApiResponse<DashboardAlertsDto>;
export type PendingTasksResponse = ApiResponse<PendingTasksDto>;
export type WarehouseStatsResponse = ApiResponse<WarehouseStatsDto[]>;
export type SingleWarehouseStatsResponse = ApiResponse<WarehouseStatsDto>;
