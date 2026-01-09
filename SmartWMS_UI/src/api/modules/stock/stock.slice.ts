/**
 * Stock API Slice
 */

import { baseApi } from '@/api/baseApi';
import type {
  StockLevelListResponse,
  StockLevelResponse,
  StockLevelFilters,
  ProductStockSummaryResponse,
  StockMovementListResponse,
  StockMovementApiResponse,
  StockMovementFilters,
  LowStockListResponse,
  AvailableQuantityResponse,
  ReceiveStockRequest,
  IssueStockRequest,
  TransferStockRequest,
  AdjustStockRequest,
  ReserveStockRequest,
  ReleaseReservationRequest,
} from './stock.types';

export const stockApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Stock Levels
    // ========================================================================

    getStockLevels: builder.query<StockLevelListResponse, StockLevelFilters | void>({
      query: (params) => ({
        url: `/stock/levels`,
        params: params || {},
      }),
      providesTags: ['Inventory'],
    }),

    getStockLevelById: builder.query<StockLevelResponse, string>({
      query: (id) => `/stock/levels/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'Inventory', id }],
    }),

    getProductStockSummary: builder.query<ProductStockSummaryResponse, string>({
      query: (productId) => `/stock/products/${productId}/summary`,
      providesTags: ['Inventory'],
    }),

    getLowStockProducts: builder.query<LowStockListResponse, string | void>({
      query: (warehouseId) => ({
        url: `/stock/low-stock`,
        params: warehouseId ? { warehouseId } : {},
      }),
      providesTags: ['Inventory'],
    }),

    getAvailableQuantity: builder.query<
      AvailableQuantityResponse,
      { productId: string; locationId?: string; batchNumber?: string }
    >({
      query: ({ productId, locationId, batchNumber }) => ({
        url: `/stock/products/${productId}/available`,
        params: { locationId, batchNumber },
      }),
      providesTags: ['Inventory'],
    }),

    // ========================================================================
    // Stock Movements
    // ========================================================================

    getStockMovements: builder.query<StockMovementListResponse, StockMovementFilters | void>({
      query: (params) => ({
        url: `/stock/movements`,
        params: params || {},
      }),
      providesTags: ['Inventory'],
    }),

    getProductMovementHistory: builder.query<
      StockMovementListResponse,
      { productId: string; limit?: number }
    >({
      query: ({ productId, limit }) => ({
        url: `/stock/products/${productId}/movements`,
        params: limit ? { limit } : {},
      }),
      providesTags: ['Inventory'],
    }),

    // ========================================================================
    // Stock Operations
    // ========================================================================

    receiveStock: builder.mutation<StockMovementApiResponse, ReceiveStockRequest>({
      query: (body) => ({
        url: `/stock/receive`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Inventory'],
    }),

    issueStock: builder.mutation<StockMovementApiResponse, IssueStockRequest>({
      query: (body) => ({
        url: `/stock/issue`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Inventory'],
    }),

    transferStock: builder.mutation<StockMovementApiResponse, TransferStockRequest>({
      query: (body) => ({
        url: `/stock/transfer`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Inventory'],
    }),

    adjustStock: builder.mutation<StockMovementApiResponse, AdjustStockRequest>({
      query: (body) => ({
        url: `/stock/adjust`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Inventory', 'Adjustments'],
    }),

    // ========================================================================
    // Reservations
    // ========================================================================

    reserveStock: builder.mutation<StockMovementApiResponse, ReserveStockRequest>({
      query: (body) => ({
        url: `/stock/reserve`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Inventory'],
    }),

    releaseReservation: builder.mutation<StockMovementApiResponse, ReleaseReservationRequest>({
      query: (body) => ({
        url: `/stock/release-reservation`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Inventory'],
    }),
  }),
});

export const {
  // Stock Levels
  useGetStockLevelsQuery,
  useGetStockLevelByIdQuery,
  useGetProductStockSummaryQuery,
  useGetLowStockProductsQuery,
  useGetAvailableQuantityQuery,
  // Stock Movements
  useGetStockMovementsQuery,
  useGetProductMovementHistoryQuery,
  // Stock Operations
  useReceiveStockMutation,
  useIssueStockMutation,
  useTransferStockMutation,
  useAdjustStockMutation,
  // Reservations
  useReserveStockMutation,
  useReleaseReservationMutation,
} = stockApi;
