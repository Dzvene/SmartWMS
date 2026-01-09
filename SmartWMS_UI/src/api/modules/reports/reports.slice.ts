/**
 * Reports API Slice
 * RTK Query endpoints for Reports module
 *
 * Endpoints:
 * - /reports/inventory-summary - Inventory summary report
 * - /reports/stock-movements - Stock movement report
 * - /reports/order-fulfillment - Order fulfillment report
 * - /reports/receiving - Receiving report
 * - /reports/warehouse-utilization/:warehouseId - Warehouse utilization
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type {
  InventorySummaryParams,
  InventorySummaryResponse,
  ReportDateRangeParams,
  StockMovementResponse,
  OrderFulfillmentResponse,
  ReceivingReportResponse,
  WarehouseUtilizationResponse,
} from './reports.types';

export const reportsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Inventory Summary Report
    // ========================================================================

    getInventorySummary: builder.query<InventorySummaryResponse, InventorySummaryParams | void>({
      query: (params: InventorySummaryParams = {}) => ({
        url: `/reports/inventory-summary`,
        params: {
          warehouseId: params.warehouseId,
        },
      }),
    }),

    // ========================================================================
    // Stock Movement Report
    // ========================================================================

    getStockMovementReport: builder.query<StockMovementResponse, ReportDateRangeParams | void>({
      query: (params: ReportDateRangeParams = {}) => ({
        url: `/reports/stock-movements`,
        params: {
          dateFrom: params.dateFrom,
          dateTo: params.dateTo,
          warehouseId: params.warehouseId,
        },
      }),
    }),

    // ========================================================================
    // Order Fulfillment Report
    // ========================================================================

    getOrderFulfillmentReport: builder.query<OrderFulfillmentResponse, ReportDateRangeParams | void>({
      query: (params: ReportDateRangeParams = {}) => ({
        url: `/reports/order-fulfillment`,
        params: {
          dateFrom: params.dateFrom,
          dateTo: params.dateTo,
        },
      }),
    }),

    // ========================================================================
    // Receiving Report
    // ========================================================================

    getReceivingReport: builder.query<ReceivingReportResponse, ReportDateRangeParams | void>({
      query: (params: ReportDateRangeParams = {}) => ({
        url: `/reports/receiving`,
        params: {
          dateFrom: params.dateFrom,
          dateTo: params.dateTo,
        },
      }),
    }),

    // ========================================================================
    // Warehouse Utilization Report
    // ========================================================================

    getWarehouseUtilization: builder.query<WarehouseUtilizationResponse, string>({
      query: (warehouseId: string) => `/reports/warehouse-utilization/${warehouseId}`,
    }),
  }),
});

// Export hooks
export const {
  useGetInventorySummaryQuery,
  useLazyGetInventorySummaryQuery,
  useGetStockMovementReportQuery,
  useLazyGetStockMovementReportQuery,
  useGetOrderFulfillmentReportQuery,
  useLazyGetOrderFulfillmentReportQuery,
  useGetReceivingReportQuery,
  useLazyGetReceivingReportQuery,
  useGetWarehouseUtilizationQuery,
  useLazyGetWarehouseUtilizationQuery,
} = reportsApi;
