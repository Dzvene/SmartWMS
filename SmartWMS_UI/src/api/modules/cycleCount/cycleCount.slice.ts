/**
 * CycleCount API Slice
 */

import { baseApi } from '@/api/baseApi';
import type {
  CycleCountSessionListResponse,
  CycleCountSessionResponse,
  CycleCountFilters,
  CreateCycleCountRequest,
  RecordCountRequest,
} from './cycleCount.types';

export const cycleCountApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getCycleCounts: builder.query<CycleCountSessionListResponse, CycleCountFilters | void>({
      query: (params) => ({
        url: `/cycle-counts`,
        params: params || {},
      }),
      providesTags: ['CycleCounts'],
    }),

    getCycleCountById: builder.query<CycleCountSessionResponse, string>({
      query: (id) => `/cycle-counts/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'CycleCounts', id }],
    }),

    createCycleCount: builder.mutation<CycleCountSessionResponse, CreateCycleCountRequest>({
      query: (body) => ({ url: `/cycle-counts`, method: 'POST', body }),
      invalidatesTags: ['CycleCounts'],
    }),

    scheduleCycleCount: builder.mutation<CycleCountSessionResponse, { id: string; scheduledDate: string }>({
      query: ({ id, scheduledDate }) => ({
        url: `/cycle-counts/${id}/schedule`,
        method: 'POST',
        body: { scheduledDate },
      }),
      invalidatesTags: ['CycleCounts'],
    }),

    assignCycleCount: builder.mutation<CycleCountSessionResponse, { id: string; userId: string }>({
      query: ({ id, userId }) => ({
        url: `/cycle-counts/${id}/assign`,
        method: 'POST',
        body: { userId },
      }),
      invalidatesTags: ['CycleCounts'],
    }),

    startCycleCount: builder.mutation<CycleCountSessionResponse, string>({
      query: (id) => ({ url: `/cycle-counts/${id}/start`, method: 'POST' }),
      invalidatesTags: ['CycleCounts'],
    }),

    recordCount: builder.mutation<CycleCountSessionResponse, { id: string; data: RecordCountRequest }>({
      query: ({ id, data }) => ({
        url: `/cycle-counts/${id}/record`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['CycleCounts'],
    }),

    completeCycleCount: builder.mutation<CycleCountSessionResponse, { id: string; notes?: string }>({
      query: ({ id, notes }) => ({
        url: `/cycle-counts/${id}/complete`,
        method: 'POST',
        body: { notes },
      }),
      invalidatesTags: ['CycleCounts', 'Inventory'],
    }),

    cancelCycleCount: builder.mutation<CycleCountSessionResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/cycle-counts/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: ['CycleCounts'],
    }),
  }),
});

export const {
  useGetCycleCountsQuery,
  useGetCycleCountByIdQuery,
  useCreateCycleCountMutation,
  useScheduleCycleCountMutation,
  useAssignCycleCountMutation,
  useStartCycleCountMutation,
  useRecordCountMutation,
  useCompleteCycleCountMutation,
  useCancelCycleCountMutation,
} = cycleCountApi;
