/**
 * Dashboard API Slice
 * RTK Query endpoints for Dashboard module
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type {
  DashboardQueryParams,
  TrendQueryParams,
  DashboardOverviewResponse,
  QuickStatsResponse,
  KpiMetricsResponse,
  KpiTrendResponse,
  OrdersTrendResponse,
  InventoryTrendResponse,
  FulfillmentTrendResponse,
  ActivityFeedResponse,
  AlertsResponse,
  PendingTasksResponse,
  WarehouseStatsResponse,
  SingleWarehouseStatsResponse,
} from './dashboard.types';

export const dashboardApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Overview
    // ========================================================================

    getDashboardOverview: builder.query<DashboardOverviewResponse, DashboardQueryParams | void>({
      query: (params: DashboardQueryParams = {}) => ({
        url: `/dashboard/overview`,
        params,
      }),
    }),

    getQuickStats: builder.query<QuickStatsResponse, void>({
      query: () => `/dashboard/quick-stats`,
    }),

    // ========================================================================
    // KPIs
    // ========================================================================

    getKpiMetrics: builder.query<KpiMetricsResponse, DashboardQueryParams | void>({
      query: (params: DashboardQueryParams = {}) => ({
        url: `/dashboard/kpi`,
        params,
      }),
    }),

    getKpiTrend: builder.query<KpiTrendResponse, { kpiCode: string; query?: TrendQueryParams }>({
      query: ({ kpiCode, query }) => ({
        url: `/dashboard/kpi/${kpiCode}/trend`,
        params: query,
      }),
    }),

    // ========================================================================
    // Trends
    // ========================================================================

    getOrdersTrend: builder.query<OrdersTrendResponse, TrendQueryParams | void>({
      query: (params: TrendQueryParams = {}) => ({
        url: `/dashboard/trends/orders`,
        params,
      }),
    }),

    getInventoryTrend: builder.query<InventoryTrendResponse, TrendQueryParams | void>({
      query: (params: TrendQueryParams = {}) => ({
        url: `/dashboard/trends/inventory`,
        params,
      }),
    }),

    getFulfillmentTrend: builder.query<FulfillmentTrendResponse, TrendQueryParams | void>({
      query: (params: TrendQueryParams = {}) => ({
        url: `/dashboard/trends/fulfillment`,
        params,
      }),
    }),

    // ========================================================================
    // Activity & Alerts
    // ========================================================================

    getActivityFeed: builder.query<ActivityFeedResponse, { limit?: number } | void>({
      query: (params) => ({
        url: `/dashboard/activity`,
        params: params || {},
      }),
    }),

    getAlerts: builder.query<AlertsResponse, void>({
      query: () => `/dashboard/alerts`,
    }),

    getPendingTasks: builder.query<PendingTasksResponse, { userId?: string } | void>({
      query: (params) => ({
        url: `/dashboard/tasks/pending`,
        params: params || {},
      }),
    }),

    // ========================================================================
    // Warehouse Stats
    // ========================================================================

    getWarehouseStats: builder.query<WarehouseStatsResponse, void>({
      query: () => `/dashboard/warehouses/stats`,
    }),

    getWarehouseStatsById: builder.query<SingleWarehouseStatsResponse, string>({
      query: (warehouseId) => `/dashboard/warehouses/${warehouseId}/stats`,
    }),
  }),
});

// Export hooks
export const {
  // Overview
  useGetDashboardOverviewQuery,
  useLazyGetDashboardOverviewQuery,
  useGetQuickStatsQuery,
  useLazyGetQuickStatsQuery,

  // KPIs
  useGetKpiMetricsQuery,
  useLazyGetKpiMetricsQuery,
  useGetKpiTrendQuery,
  useLazyGetKpiTrendQuery,

  // Trends
  useGetOrdersTrendQuery,
  useLazyGetOrdersTrendQuery,
  useGetInventoryTrendQuery,
  useLazyGetInventoryTrendQuery,
  useGetFulfillmentTrendQuery,
  useLazyGetFulfillmentTrendQuery,

  // Activity & Alerts
  useGetActivityFeedQuery,
  useLazyGetActivityFeedQuery,
  useGetAlertsQuery,
  useLazyGetAlertsQuery,
  useGetPendingTasksQuery,
  useLazyGetPendingTasksQuery,

  // Warehouse
  useGetWarehouseStatsQuery,
  useLazyGetWarehouseStatsQuery,
  useGetWarehouseStatsByIdQuery,
  useLazyGetWarehouseStatsByIdQuery,
} = dashboardApi;
