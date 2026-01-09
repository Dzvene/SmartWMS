/**
 * Equipment API Slice
 * RTK Query endpoints for Equipment module
 */

import { baseApi } from '@/api/baseApi';
import type {
  EquipmentFilters,
  EquipmentResponse,
  EquipmentListResponse,
  CreateEquipmentRequest,
  UpdateEquipmentRequest,
  AssignEquipmentRequest,
  UpdateEquipmentStatusRequest,
} from './equipment.types';

export const equipmentApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Equipment CRUD
    // ========================================================================

    getEquipment: builder.query<EquipmentListResponse, EquipmentFilters | void>({
      query: (params) => ({
        url: '/equipment',
        params: params || {},
      }),
      providesTags: ['Equipment'],
    }),

    getEquipmentById: builder.query<EquipmentResponse, string>({
      query: (id) => `/equipment/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Equipment', id }],
    }),

    createEquipment: builder.mutation<EquipmentResponse, CreateEquipmentRequest>({
      query: (body) => ({
        url: '/equipment',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Equipment'],
    }),

    updateEquipment: builder.mutation<EquipmentResponse, { id: string; body: UpdateEquipmentRequest }>({
      query: ({ id, body }) => ({
        url: `/equipment/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Equipment', id }, 'Equipment'],
    }),

    deleteEquipment: builder.mutation<void, string>({
      query: (id) => ({
        url: `/equipment/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Equipment'],
    }),

    // ========================================================================
    // Equipment Actions
    // ========================================================================

    assignEquipment: builder.mutation<EquipmentResponse, { id: string; body: AssignEquipmentRequest }>({
      query: ({ id, body }) => ({
        url: `/equipment/${id}/assign`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Equipment', id }, 'Equipment'],
    }),

    unassignEquipment: builder.mutation<EquipmentResponse, string>({
      query: (id) => ({
        url: `/equipment/${id}/unassign`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'Equipment', id }, 'Equipment'],
    }),

    updateEquipmentStatus: builder.mutation<EquipmentResponse, { id: string; body: UpdateEquipmentStatusRequest }>({
      query: ({ id, body }) => ({
        url: `/equipment/${id}/status`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Equipment', id }, 'Equipment'],
    }),

    // ========================================================================
    // Equipment Maintenance
    // ========================================================================

    scheduleMaintenace: builder.mutation<EquipmentResponse, { id: string; nextMaintenanceDate: string; notes?: string }>({
      query: ({ id, ...body }) => ({
        url: `/equipment/${id}/maintenance/schedule`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Equipment', id }],
    }),

    completeMaintenance: builder.mutation<EquipmentResponse, { id: string; notes?: string }>({
      query: ({ id, notes }) => ({
        url: `/equipment/${id}/maintenance/complete`,
        method: 'POST',
        body: { notes },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Equipment', id }, 'Equipment'],
    }),

    // ========================================================================
    // Queries by Type/Status
    // ========================================================================

    getAvailableEquipment: builder.query<EquipmentListResponse, { warehouseId?: string; type?: string } | void>({
      query: (params) => ({
        url: '/equipment/available',
        params: params || {},
      }),
      providesTags: ['Equipment'],
    }),

    getEquipmentByWarehouse: builder.query<EquipmentListResponse, string>({
      query: (warehouseId) => `/equipment/warehouse/${warehouseId}`,
      providesTags: ['Equipment'],
    }),

    getEquipmentRequiringMaintenance: builder.query<EquipmentListResponse, void>({
      query: () => '/equipment/maintenance-required',
      providesTags: ['Equipment'],
    }),
  }),
});

// Export hooks
export const {
  // CRUD
  useGetEquipmentQuery,
  useLazyGetEquipmentQuery,
  useGetEquipmentByIdQuery,
  useLazyGetEquipmentByIdQuery,
  useCreateEquipmentMutation,
  useUpdateEquipmentMutation,
  useDeleteEquipmentMutation,

  // Actions
  useAssignEquipmentMutation,
  useUnassignEquipmentMutation,
  useUpdateEquipmentStatusMutation,

  // Maintenance
  useScheduleMaintenaceMutation,
  useCompleteMaintenanceMutation,

  // Special Queries
  useGetAvailableEquipmentQuery,
  useLazyGetAvailableEquipmentQuery,
  useGetEquipmentByWarehouseQuery,
  useLazyGetEquipmentByWarehouseQuery,
  useGetEquipmentRequiringMaintenanceQuery,
  useLazyGetEquipmentRequiringMaintenanceQuery,
} = equipmentApi;
