/**
 * Receiving API Slice
 * RTK Query endpoints for Receiving module (Goods Receipts)
 */

import { baseApi } from '@/api/baseApi';
import type {
  GoodsReceiptFilters,
  GoodsReceiptResponse,
  GoodsReceiptListResponse,
  GoodsReceiptLineResponse,
  CreateGoodsReceiptRequest,
  UpdateGoodsReceiptRequest,
  AddGoodsReceiptLineRequest,
  ReceiveLineRequest,
} from './receiving.types';

export const receivingApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Goods Receipts CRUD
    // ========================================================================

    getGoodsReceipts: builder.query<GoodsReceiptListResponse, GoodsReceiptFilters | void>({
      query: (params) => ({
        url: '/receiving/goods-receipts',
        params: params || {},
      }),
      providesTags: ['GoodsReceipts'],
    }),

    getGoodsReceiptById: builder.query<GoodsReceiptResponse, string>({
      query: (id) => `/receiving/goods-receipts/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'GoodsReceipts', id }],
    }),

    createGoodsReceipt: builder.mutation<GoodsReceiptResponse, CreateGoodsReceiptRequest>({
      query: (body) => ({
        url: '/receiving/goods-receipts',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['GoodsReceipts'],
    }),

    updateGoodsReceipt: builder.mutation<GoodsReceiptResponse, { id: string; body: UpdateGoodsReceiptRequest }>({
      query: ({ id, body }) => ({
        url: `/receiving/goods-receipts/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'GoodsReceipts', id }, 'GoodsReceipts'],
    }),

    deleteGoodsReceipt: builder.mutation<void, string>({
      query: (id) => ({
        url: `/receiving/goods-receipts/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['GoodsReceipts'],
    }),

    // ========================================================================
    // Goods Receipt Lines
    // ========================================================================

    addGoodsReceiptLine: builder.mutation<GoodsReceiptLineResponse, { receiptId: string; body: AddGoodsReceiptLineRequest }>({
      query: ({ receiptId, body }) => ({
        url: `/receiving/goods-receipts/${receiptId}/lines`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { receiptId }) => [{ type: 'GoodsReceipts', id: receiptId }],
    }),

    deleteGoodsReceiptLine: builder.mutation<void, { receiptId: string; lineId: string }>({
      query: ({ receiptId, lineId }) => ({
        url: `/receiving/goods-receipts/${receiptId}/lines/${lineId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, { receiptId }) => [{ type: 'GoodsReceipts', id: receiptId }],
    }),

    // ========================================================================
    // Receiving Operations
    // ========================================================================

    receiveLine: builder.mutation<GoodsReceiptLineResponse, { receiptId: string; lineId: string; body: ReceiveLineRequest }>({
      query: ({ receiptId, lineId, body }) => ({
        url: `/receiving/goods-receipts/${receiptId}/lines/${lineId}/receive`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { receiptId }) => [{ type: 'GoodsReceipts', id: receiptId }, 'GoodsReceipts', 'PurchaseOrders'],
    }),

    // ========================================================================
    // Goods Receipt Actions
    // ========================================================================

    startReceiving: builder.mutation<GoodsReceiptResponse, string>({
      query: (id) => ({
        url: `/receiving/goods-receipts/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'GoodsReceipts', id }, 'GoodsReceipts'],
    }),

    completeGoodsReceipt: builder.mutation<GoodsReceiptResponse, string>({
      query: (id) => ({
        url: `/receiving/goods-receipts/${id}/complete`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'GoodsReceipts', id }, 'GoodsReceipts', 'PurchaseOrders'],
    }),

    cancelGoodsReceipt: builder.mutation<GoodsReceiptResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/receiving/goods-receipts/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'GoodsReceipts', id }, 'GoodsReceipts'],
    }),

    // ========================================================================
    // Create from PO
    // ========================================================================

    createFromPurchaseOrder: builder.mutation<GoodsReceiptResponse, { purchaseOrderId: string; warehouseId?: string }>({
      query: ({ purchaseOrderId, warehouseId }) => ({
        url: '/receiving/goods-receipts/from-purchase-order',
        method: 'POST',
        body: { purchaseOrderId, warehouseId },
      }),
      invalidatesTags: ['GoodsReceipts'],
    }),

    // ========================================================================
    // Queries
    // ========================================================================

    getPendingReceipts: builder.query<GoodsReceiptListResponse, { warehouseId?: string } | void>({
      query: (params) => ({
        url: '/receiving/goods-receipts/pending',
        params: params || {},
      }),
      providesTags: ['GoodsReceipts'],
    }),

    getReceiptsByPurchaseOrder: builder.query<GoodsReceiptListResponse, string>({
      query: (purchaseOrderId) => `/receiving/goods-receipts/by-purchase-order/${purchaseOrderId}`,
      providesTags: ['GoodsReceipts'],
    }),
  }),
});

// Export hooks
export const {
  // CRUD
  useGetGoodsReceiptsQuery,
  useLazyGetGoodsReceiptsQuery,
  useGetGoodsReceiptByIdQuery,
  useLazyGetGoodsReceiptByIdQuery,
  useCreateGoodsReceiptMutation,
  useUpdateGoodsReceiptMutation,
  useDeleteGoodsReceiptMutation,

  // Lines
  useAddGoodsReceiptLineMutation,
  useDeleteGoodsReceiptLineMutation,

  // Operations
  useReceiveLineMutation,

  // Actions
  useStartReceivingMutation,
  useCompleteGoodsReceiptMutation,
  useCancelGoodsReceiptMutation,

  // Create from PO
  useCreateFromPurchaseOrderMutation,

  // Queries
  useGetPendingReceiptsQuery,
  useLazyGetPendingReceiptsQuery,
  useGetReceiptsByPurchaseOrderQuery,
  useLazyGetReceiptsByPurchaseOrderQuery,
} = receivingApi;
