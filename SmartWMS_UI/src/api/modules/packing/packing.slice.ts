/**
 * Packing API Slice
 * RTK Query endpoints for Packing module
 */

import { baseApi } from '@/api/baseApi';
import type {
  PackingTaskListResponse,
  PackingTaskResponse,
  PackingTaskFilters,
  CreatePackingTaskRequest,
  AssignPackingTaskRequest,
  CreatePackageRequest,
  AddItemToPackageRequest,
  CompletePackingTaskRequest,
  PackageResponse,
  PackingStationListResponse,
} from './packing.types';
import type { ApiResponse, PaginationParams } from '@/api/types';

export const packingApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Packing Tasks
    // ========================================================================

    getPackingTasks: builder.query<PackingTaskListResponse, PackingTaskFilters | void>({
      query: (params: PackingTaskFilters = {}) => ({
        url: `/packing/tasks`,
        params,
      }),
      providesTags: (result) =>
        result?.data?.items
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'PackingTasks' as const, id })),
              { type: 'PackingTasks', id: 'LIST' },
            ]
          : [{ type: 'PackingTasks', id: 'LIST' }],
    }),

    getPackingTaskById: builder.query<PackingTaskResponse, string>({
      query: (id) => `/packing/tasks/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'PackingTasks', id }],
    }),

    createPackingTask: builder.mutation<PackingTaskResponse, CreatePackingTaskRequest>({
      query: (body) => ({
        url: `/packing/tasks`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'PackingTasks', id: 'LIST' }],
    }),

    assignPackingTask: builder.mutation<PackingTaskResponse, { id: string; data: AssignPackingTaskRequest }>({
      query: ({ id, data }) => ({
        url: `/packing/tasks/${id}/assign`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'PackingTasks', id },
        { type: 'PackingTasks', id: 'LIST' },
      ],
    }),

    startPackingTask: builder.mutation<PackingTaskResponse, string>({
      query: (id) => ({
        url: `/packing/tasks/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'PackingTasks', id },
        { type: 'PackingTasks', id: 'LIST' },
      ],
    }),

    completePackingTask: builder.mutation<PackingTaskResponse, { id: string; data?: CompletePackingTaskRequest }>({
      query: ({ id, data }) => ({
        url: `/packing/tasks/${id}/complete`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'PackingTasks', id },
        { type: 'PackingTasks', id: 'LIST' },
        { type: 'Shipments', id: 'LIST' },
      ],
    }),

    cancelPackingTask: builder.mutation<PackingTaskResponse, string>({
      query: (id) => ({
        url: `/packing/tasks/${id}/cancel`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'PackingTasks', id },
        { type: 'PackingTasks', id: 'LIST' },
      ],
    }),

    // ========================================================================
    // Packages
    // ========================================================================

    createPackage: builder.mutation<PackageResponse, { taskId: string; data: CreatePackageRequest }>({
      query: ({ taskId, data }) => ({
        url: `/packing/tasks/${taskId}/packages`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { taskId }) => [{ type: 'PackingTasks', id: taskId }],
    }),

    addItemToPackage: builder.mutation<ApiResponse<void>, { packageId: string; data: AddItemToPackageRequest }>({
      query: ({ packageId, data }) => ({
        url: `/packing/packages/${packageId}/items`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: [{ type: 'PackingTasks', id: 'LIST' }],
    }),

    // ========================================================================
    // Packing Stations
    // ========================================================================

    getPackingStations: builder.query<PackingStationListResponse, PaginationParams | void>({
      query: (params) => ({
        url: `/packing/stations`,
        params: params || {},
      }),
      providesTags: ['PackingStations'],
    }),
  }),
});

export const {
  useGetPackingTasksQuery,
  useLazyGetPackingTasksQuery,
  useGetPackingTaskByIdQuery,
  useCreatePackingTaskMutation,
  useAssignPackingTaskMutation,
  useStartPackingTaskMutation,
  useCompletePackingTaskMutation,
  useCancelPackingTaskMutation,
  useCreatePackageMutation,
  useAddItemToPackageMutation,
  useGetPackingStationsQuery,
} = packingApi;
