/**
 * Fulfillment API Slice
 * RTK Query endpoints for Fulfillment module (Pick Tasks, Batches, Shipments)
 */

import { baseApi } from '@/api/baseApi';
import type {
  // Pick Tasks
  PickTaskFilters,
  PickTaskResponse,
  PickTaskListResponse,
  CreatePickTaskRequest,
  AssignPickTaskRequest,
  ConfirmPickRequest,
  ShortPickRequest,
  // Batches
  BatchFilters,
  FulfillmentBatchResponse,
  FulfillmentBatchListResponse,
  CreateBatchRequest,
  // Shipments
  ShipmentFilters,
  ShipmentResponse,
  ShipmentListResponse,
  CreateShipmentRequest,
  UpdateShipmentRequest,
  ShipShipmentRequest,
} from './fulfillment.types';

export const fulfillmentApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Pick Tasks
    // ========================================================================

    getPickTasks: builder.query<PickTaskListResponse, PickTaskFilters | void>({
      query: (params) => ({
        url: '/fulfillment/pick-tasks',
        params: params || {},
      }),
      providesTags: ['PickTasks'],
    }),

    getPickTaskById: builder.query<PickTaskResponse, string>({
      query: (id) => `/fulfillment/pick-tasks/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'PickTasks', id }],
    }),

    createPickTask: builder.mutation<PickTaskResponse, CreatePickTaskRequest>({
      query: (body) => ({
        url: '/fulfillment/pick-tasks',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['PickTasks'],
    }),

    assignPickTask: builder.mutation<PickTaskResponse, { id: string; body: AssignPickTaskRequest }>({
      query: ({ id, body }) => ({
        url: `/fulfillment/pick-tasks/${id}/assign`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PickTasks', id }, 'PickTasks'],
    }),

    startPickTask: builder.mutation<PickTaskResponse, string>({
      query: (id) => ({
        url: `/fulfillment/pick-tasks/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'PickTasks', id }, 'PickTasks'],
    }),

    confirmPick: builder.mutation<PickTaskResponse, { id: string; body: ConfirmPickRequest }>({
      query: ({ id, body }) => ({
        url: `/fulfillment/pick-tasks/${id}/confirm`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PickTasks', id }, 'PickTasks', 'SalesOrders'],
    }),

    shortPick: builder.mutation<PickTaskResponse, { id: string; body: ShortPickRequest }>({
      query: ({ id, body }) => ({
        url: `/fulfillment/pick-tasks/${id}/short-pick`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PickTasks', id }, 'PickTasks'],
    }),

    cancelPickTask: builder.mutation<PickTaskResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/fulfillment/pick-tasks/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PickTasks', id }, 'PickTasks'],
    }),

    // ========================================================================
    // Fulfillment Batches
    // ========================================================================

    getBatches: builder.query<FulfillmentBatchListResponse, BatchFilters | void>({
      query: (params) => ({
        url: '/fulfillment/batches',
        params: params || {},
      }),
      providesTags: ['FulfillmentBatches'],
    }),

    getBatchById: builder.query<FulfillmentBatchResponse, string>({
      query: (id) => `/fulfillment/batches/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'FulfillmentBatches', id }],
    }),

    createBatch: builder.mutation<FulfillmentBatchResponse, CreateBatchRequest>({
      query: (body) => ({
        url: '/fulfillment/batches',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['FulfillmentBatches', 'PickTasks'],
    }),

    assignBatch: builder.mutation<FulfillmentBatchResponse, { id: string; userId: string }>({
      query: ({ id, userId }) => ({
        url: `/fulfillment/batches/${id}/assign`,
        method: 'POST',
        body: { userId },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'FulfillmentBatches', id }, 'FulfillmentBatches'],
    }),

    startBatch: builder.mutation<FulfillmentBatchResponse, string>({
      query: (id) => ({
        url: `/fulfillment/batches/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'FulfillmentBatches', id }, 'FulfillmentBatches', 'PickTasks'],
    }),

    completeBatch: builder.mutation<FulfillmentBatchResponse, string>({
      query: (id) => ({
        url: `/fulfillment/batches/${id}/complete`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'FulfillmentBatches', id }, 'FulfillmentBatches'],
    }),

    cancelBatch: builder.mutation<FulfillmentBatchResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/fulfillment/batches/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'FulfillmentBatches', id }, 'FulfillmentBatches', 'PickTasks'],
    }),

    // ========================================================================
    // Shipments
    // ========================================================================

    getShipments: builder.query<ShipmentListResponse, ShipmentFilters | void>({
      query: (params) => ({
        url: '/fulfillment/shipments',
        params: params || {},
      }),
      providesTags: ['Shipments'],
    }),

    getShipmentById: builder.query<ShipmentResponse, string>({
      query: (id) => `/fulfillment/shipments/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Shipments', id }],
    }),

    createShipment: builder.mutation<ShipmentResponse, CreateShipmentRequest>({
      query: (body) => ({
        url: '/fulfillment/shipments',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Shipments', 'SalesOrders'],
    }),

    updateShipment: builder.mutation<ShipmentResponse, { id: string; body: UpdateShipmentRequest }>({
      query: ({ id, body }) => ({
        url: `/fulfillment/shipments/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Shipments', id }, 'Shipments'],
    }),

    shipShipment: builder.mutation<ShipmentResponse, { id: string; body?: ShipShipmentRequest }>({
      query: ({ id, body }) => ({
        url: `/fulfillment/shipments/${id}/ship`,
        method: 'POST',
        body: body || {},
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Shipments', id }, 'Shipments', 'SalesOrders'],
    }),

    markDelivered: builder.mutation<ShipmentResponse, { id: string; deliveredAt?: string }>({
      query: ({ id, deliveredAt }) => ({
        url: `/fulfillment/shipments/${id}/deliver`,
        method: 'POST',
        body: { deliveredAt },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Shipments', id }, 'Shipments', 'SalesOrders'],
    }),

    cancelShipment: builder.mutation<ShipmentResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/fulfillment/shipments/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Shipments', id }, 'Shipments'],
    }),

    // ========================================================================
    // Tracking
    // ========================================================================

    getShipmentTracking: builder.query<{ trackingUrl: string; events: unknown[] }, string>({
      query: (id) => `/fulfillment/shipments/${id}/tracking`,
    }),
  }),
});

// Export hooks
export const {
  // Pick Tasks
  useGetPickTasksQuery,
  useLazyGetPickTasksQuery,
  useGetPickTaskByIdQuery,
  useLazyGetPickTaskByIdQuery,
  useCreatePickTaskMutation,
  useAssignPickTaskMutation,
  useStartPickTaskMutation,
  useConfirmPickMutation,
  useShortPickMutation,
  useCancelPickTaskMutation,

  // Batches
  useGetBatchesQuery,
  useLazyGetBatchesQuery,
  useGetBatchByIdQuery,
  useLazyGetBatchByIdQuery,
  useCreateBatchMutation,
  useAssignBatchMutation,
  useStartBatchMutation,
  useCompleteBatchMutation,
  useCancelBatchMutation,

  // Shipments
  useGetShipmentsQuery,
  useLazyGetShipmentsQuery,
  useGetShipmentByIdQuery,
  useLazyGetShipmentByIdQuery,
  useCreateShipmentMutation,
  useUpdateShipmentMutation,
  useShipShipmentMutation,
  useMarkDeliveredMutation,
  useCancelShipmentMutation,

  // Tracking
  useGetShipmentTrackingQuery,
  useLazyGetShipmentTrackingQuery,
} = fulfillmentApi;
