/**
 * Transfers API Slice
 */

import { baseApi } from '@/api/baseApi';
import type {
  StockTransferListResponse,
  StockTransferResponse,
  TransferFilters,
  CreateTransferRequest,
  PickTransferLineRequest,
  ReceiveTransferLineRequest,
} from './transfers.types';

export const transfersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getTransfers: builder.query<StockTransferListResponse, TransferFilters | void>({
      query: (params) => ({
        url: `/transfers`,
        params: params || {},
      }),
      providesTags: ['Transfers'],
    }),

    getTransferById: builder.query<StockTransferResponse, string>({
      query: (id) => `/transfers/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'Transfers', id }],
    }),

    createTransfer: builder.mutation<StockTransferResponse, CreateTransferRequest>({
      query: (body) => ({ url: `/transfers`, method: 'POST', body }),
      invalidatesTags: ['Transfers'],
    }),

    releaseTransfer: builder.mutation<StockTransferResponse, string>({
      query: (id) => ({ url: `/transfers/${id}/release`, method: 'POST' }),
      invalidatesTags: ['Transfers'],
    }),

    assignTransfer: builder.mutation<StockTransferResponse, { id: string; userId: string }>({
      query: ({ id, userId }) => ({
        url: `/transfers/${id}/assign`,
        method: 'POST',
        body: { userId },
      }),
      invalidatesTags: ['Transfers'],
    }),

    startTransfer: builder.mutation<StockTransferResponse, string>({
      query: (id) => ({ url: `/transfers/${id}/start`, method: 'POST' }),
      invalidatesTags: ['Transfers'],
    }),

    pickTransferLine: builder.mutation<StockTransferResponse, { id: string; data: PickTransferLineRequest }>({
      query: ({ id, data }) => ({
        url: `/transfers/${id}/pick`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Transfers', 'Inventory'],
    }),

    receiveTransferLine: builder.mutation<StockTransferResponse, { id: string; data: ReceiveTransferLineRequest }>({
      query: ({ id, data }) => ({
        url: `/transfers/${id}/receive`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Transfers', 'Inventory'],
    }),

    completeTransfer: builder.mutation<StockTransferResponse, { id: string; notes?: string }>({
      query: ({ id, notes }) => ({
        url: `/transfers/${id}/complete`,
        method: 'POST',
        body: { notes },
      }),
      invalidatesTags: ['Transfers', 'Inventory'],
    }),

    cancelTransfer: builder.mutation<StockTransferResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/transfers/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: ['Transfers'],
    }),
  }),
});

export const {
  useGetTransfersQuery,
  useGetTransferByIdQuery,
  useCreateTransferMutation,
  useReleaseTransferMutation,
  useAssignTransferMutation,
  useStartTransferMutation,
  usePickTransferLineMutation,
  useReceiveTransferLineMutation,
  useCompleteTransferMutation,
  useCancelTransferMutation,
} = transfersApi;
