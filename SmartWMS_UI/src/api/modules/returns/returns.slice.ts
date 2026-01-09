/**
 * Returns API Slice
 * RTK Query endpoints for Returns module
 */

import { baseApi } from '@/api/baseApi';
import type {
  ReturnOrderListResponse,
  ReturnOrderResponse,
  ReturnOrderFilters,
  CreateReturnOrderRequest,
  UpdateReturnOrderRequest,
  AddReturnLineRequest,
  ReceiveReturnLineRequest,
  ProcessReturnLineRequest,
  AssignReturnRequest,
  SetReturnShippingRequest,
} from './returns.types';

export const returnsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Return Orders - Queries
    // ========================================================================

    getReturnOrders: builder.query<ReturnOrderListResponse, ReturnOrderFilters | void>({
      query: (params) => ({
        url: `/returns`,
        params: params || {},
      }),
      providesTags: (result) =>
        result?.data?.items
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'Returns' as const, id })),
              { type: 'Returns', id: 'LIST' },
            ]
          : [{ type: 'Returns', id: 'LIST' }],
    }),

    getReturnOrderById: builder.query<ReturnOrderResponse, string>({
      query: (id) => `/returns/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Returns', id }],
    }),

    getReturnOrderByNumber: builder.query<ReturnOrderResponse, string>({
      query: (returnNumber) => `/returns/by-number/${returnNumber}`,
      providesTags: (result) => (result?.data ? [{ type: 'Returns', id: result.data.id }] : []),
    }),

    getMyReturns: builder.query<ReturnOrderListResponse, string>({
      query: (userId) => `/returns/my-returns/${userId}`,
      providesTags: [{ type: 'Returns', id: 'MY_RETURNS' }],
    }),

    // ========================================================================
    // Return Orders - CRUD
    // ========================================================================

    createReturnOrder: builder.mutation<ReturnOrderResponse, CreateReturnOrderRequest>({
      query: (body) => ({
        url: `/returns`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'Returns', id: 'LIST' }],
    }),

    updateReturnOrder: builder.mutation<ReturnOrderResponse, { id: string; data: UpdateReturnOrderRequest }>({
      query: ({ id, data }) => ({
        url: `/returns/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
      ],
    }),

    deleteReturnOrder: builder.mutation<ReturnOrderResponse, string>({
      query: (id) => ({
        url: `/returns/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
      ],
    }),

    // ========================================================================
    // Return Order Lines
    // ========================================================================

    addReturnLine: builder.mutation<ReturnOrderResponse, { returnOrderId: string; data: AddReturnLineRequest }>({
      query: ({ returnOrderId, data }) => ({
        url: `/returns/${returnOrderId}/lines`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { returnOrderId }) => [{ type: 'Returns', id: returnOrderId }],
    }),

    removeReturnLine: builder.mutation<ReturnOrderResponse, { returnOrderId: string; lineId: string }>({
      query: ({ returnOrderId, lineId }) => ({
        url: `/returns/${returnOrderId}/lines/${lineId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, { returnOrderId }) => [{ type: 'Returns', id: returnOrderId }],
    }),

    // ========================================================================
    // Workflow Actions
    // ========================================================================

    assignReturn: builder.mutation<ReturnOrderResponse, { id: string; data: AssignReturnRequest }>({
      query: ({ id, data }) => ({
        url: `/returns/${id}/assign`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
        { type: 'Returns', id: 'MY_RETURNS' },
      ],
    }),

    markReturnInTransit: builder.mutation<ReturnOrderResponse, { id: string; data: SetReturnShippingRequest }>({
      query: ({ id, data }) => ({
        url: `/returns/${id}/in-transit`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
      ],
    }),

    startReceivingReturn: builder.mutation<ReturnOrderResponse, string>({
      query: (id) => ({
        url: `/returns/${id}/start-receiving`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
      ],
    }),

    receiveReturnLine: builder.mutation<
      ReturnOrderResponse,
      { returnOrderId: string; lineId: string; data: ReceiveReturnLineRequest }
    >({
      query: ({ returnOrderId, lineId, data }) => ({
        url: `/returns/${returnOrderId}/lines/${lineId}/receive`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { returnOrderId }) => [{ type: 'Returns', id: returnOrderId }],
    }),

    processReturnLine: builder.mutation<
      ReturnOrderResponse,
      { returnOrderId: string; lineId: string; data: ProcessReturnLineRequest }
    >({
      query: ({ returnOrderId, lineId, data }) => ({
        url: `/returns/${returnOrderId}/lines/${lineId}/process`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { returnOrderId }) => [{ type: 'Returns', id: returnOrderId }],
    }),

    completeReturn: builder.mutation<ReturnOrderResponse, string>({
      query: (id) => ({
        url: `/returns/${id}/complete`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
      ],
    }),

    cancelReturn: builder.mutation<ReturnOrderResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/returns/${id}/cancel`,
        method: 'POST',
        body: reason,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Returns', id },
        { type: 'Returns', id: 'LIST' },
      ],
    }),
  }),
});

export const {
  useGetReturnOrdersQuery,
  useLazyGetReturnOrdersQuery,
  useGetReturnOrderByIdQuery,
  useGetReturnOrderByNumberQuery,
  useGetMyReturnsQuery,
  useCreateReturnOrderMutation,
  useUpdateReturnOrderMutation,
  useDeleteReturnOrderMutation,
  useAddReturnLineMutation,
  useRemoveReturnLineMutation,
  useAssignReturnMutation,
  useMarkReturnInTransitMutation,
  useStartReceivingReturnMutation,
  useReceiveReturnLineMutation,
  useProcessReturnLineMutation,
  useCompleteReturnMutation,
  useCancelReturnMutation,
} = returnsApi;
