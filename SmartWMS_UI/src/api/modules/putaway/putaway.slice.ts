/**
 * Putaway API Slice
 * RTK Query endpoints for Putaway module
 */

import { baseApi } from '@/api/baseApi';
import type {
  PutawayTaskListResponse,
  PutawayTaskResponse,
  PutawayTaskFilters,
  CreatePutawayTaskRequest,
  CreatePutawayFromReceiptRequest,
  AssignPutawayTaskRequest,
  CompletePutawayTaskRequest,
  SuggestLocationRequest,
  LocationSuggestionResponse,
} from './putaway.types';
import type { ApiResponse } from '@/api/types';

export const putawayApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Putaway Tasks
    // ========================================================================

    getPutawayTasks: builder.query<PutawayTaskListResponse, PutawayTaskFilters | void>({
      query: (params: PutawayTaskFilters = {}) => ({
        url: `/putaway`,
        params,
      }),
      providesTags: (result) =>
        result?.data?.items
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'PutawayTasks' as const, id })),
              { type: 'PutawayTasks', id: 'LIST' },
            ]
          : [{ type: 'PutawayTasks', id: 'LIST' }],
    }),

    getPutawayTaskById: builder.query<PutawayTaskResponse, string>({
      query: (id) => `/putaway/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'PutawayTasks', id }],
    }),

    createPutawayTask: builder.mutation<PutawayTaskResponse, CreatePutawayTaskRequest>({
      query: (body) => ({
        url: `/putaway`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'PutawayTasks', id: 'LIST' }],
    }),

    createPutawayFromReceipt: builder.mutation<ApiResponse<{ count: number }>, CreatePutawayFromReceiptRequest>({
      query: (body) => ({
        url: `/putaway/from-goods-receipt`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'PutawayTasks', id: 'LIST' }, { type: 'GoodsReceipts', id: 'LIST' }],
    }),

    assignPutawayTask: builder.mutation<PutawayTaskResponse, { id: string; data: AssignPutawayTaskRequest }>({
      query: ({ id, data }) => ({
        url: `/putaway/${id}/assign`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'PutawayTasks', id },
        { type: 'PutawayTasks', id: 'LIST' },
      ],
    }),

    startPutawayTask: builder.mutation<PutawayTaskResponse, string>({
      query: (id) => ({
        url: `/putaway/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'PutawayTasks', id },
        { type: 'PutawayTasks', id: 'LIST' },
      ],
    }),

    completePutawayTask: builder.mutation<PutawayTaskResponse, { id: string; data: CompletePutawayTaskRequest }>({
      query: ({ id, data }) => ({
        url: `/putaway/${id}/complete`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'PutawayTasks', id },
        { type: 'PutawayTasks', id: 'LIST' },
        { type: 'Inventory', id: 'LIST' },
      ],
    }),

    cancelPutawayTask: builder.mutation<PutawayTaskResponse, string>({
      query: (id) => ({
        url: `/putaway/${id}/cancel`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'PutawayTasks', id },
        { type: 'PutawayTasks', id: 'LIST' },
      ],
    }),

    // ========================================================================
    // Location Suggestions
    // ========================================================================

    suggestLocation: builder.query<LocationSuggestionResponse, SuggestLocationRequest>({
      query: (params) => ({
        url: `/putaway/suggest-location`,
        params,
      }),
    }),
  }),
});

export const {
  useGetPutawayTasksQuery,
  useLazyGetPutawayTasksQuery,
  useGetPutawayTaskByIdQuery,
  useCreatePutawayTaskMutation,
  useCreatePutawayFromReceiptMutation,
  useAssignPutawayTaskMutation,
  useStartPutawayTaskMutation,
  useCompletePutawayTaskMutation,
  useCancelPutawayTaskMutation,
  useSuggestLocationQuery,
  useLazySuggestLocationQuery,
} = putawayApi;
