/**
 * Adjustments API Slice
 */

import { baseApi } from '@/api/baseApi';
import type {
  StockAdjustmentListResponse,
  StockAdjustmentResponse,
  AdjustmentFilters,
  CreateStockAdjustmentRequest,
} from './adjustments.types';

export const adjustmentsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getAdjustments: builder.query<StockAdjustmentListResponse, AdjustmentFilters | void>({
      query: (params) => ({
        url: `/adjustments`,
        params: params || {},
      }),
      providesTags: ['Adjustments'],
    }),

    getAdjustmentById: builder.query<StockAdjustmentResponse, string>({
      query: (id) => `/adjustments/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'Adjustments', id }],
    }),

    createAdjustment: builder.mutation<StockAdjustmentResponse, CreateStockAdjustmentRequest>({
      query: (body) => ({ url: `/adjustments`, method: 'POST', body }),
      invalidatesTags: ['Adjustments'],
    }),

    submitAdjustment: builder.mutation<StockAdjustmentResponse, { id: string; notes?: string }>({
      query: ({ id, notes }) => ({ url: `/adjustments/${id}/submit`, method: 'POST', body: { notes } }),
      invalidatesTags: ['Adjustments'],
    }),

    approveAdjustment: builder.mutation<StockAdjustmentResponse, { id: string; approvalNotes?: string }>({
      query: ({ id, approvalNotes }) => ({ url: `/adjustments/${id}/approve`, method: 'POST', body: { approvalNotes } }),
      invalidatesTags: ['Adjustments'],
    }),

    rejectAdjustment: builder.mutation<StockAdjustmentResponse, { id: string; rejectionReason: string }>({
      query: ({ id, rejectionReason }) => ({ url: `/adjustments/${id}/reject`, method: 'POST', body: { rejectionReason } }),
      invalidatesTags: ['Adjustments'],
    }),

    postAdjustment: builder.mutation<StockAdjustmentResponse, { id: string; postingNotes?: string }>({
      query: ({ id, postingNotes }) => ({ url: `/adjustments/${id}/post`, method: 'POST', body: { postingNotes } }),
      invalidatesTags: ['Adjustments', 'Inventory'],
    }),

    cancelAdjustment: builder.mutation<StockAdjustmentResponse, string>({
      query: (id) => ({ url: `/adjustments/${id}/cancel`, method: 'POST' }),
      invalidatesTags: ['Adjustments'],
    }),

    quickAdjust: builder.mutation<StockAdjustmentResponse, CreateStockAdjustmentRequest>({
      query: (body) => ({ url: `/adjustments/quick`, method: 'POST', body }),
      invalidatesTags: ['Adjustments', 'Inventory'],
    }),
  }),
});

export const {
  useGetAdjustmentsQuery,
  useGetAdjustmentByIdQuery,
  useCreateAdjustmentMutation,
  useSubmitAdjustmentMutation,
  useApproveAdjustmentMutation,
  useRejectAdjustmentMutation,
  usePostAdjustmentMutation,
  useCancelAdjustmentMutation,
  useQuickAdjustMutation,
} = adjustmentsApi;
