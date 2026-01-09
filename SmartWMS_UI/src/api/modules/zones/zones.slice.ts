/**
 * Zones API Slice
 * RTK Query endpoints for Zones module
 *
 * Endpoints:
 * - /zones - Zone CRUD
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type {
  ZonesListParams,
  ZonesListResponse,
  ZoneDetailResponse,
  CreateZoneRequest,
  UpdateZoneRequest,
} from './zones.types';
import type { ApiResponse } from '@/api/types';

export const zonesApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Zones CRUD
    // ========================================================================

    getZones: builder.query<ZonesListResponse, ZonesListParams | void>({
      query: ({
        warehouseId,
        page = 1,
        pageSize = 100,
        search,
        isActive,
      }: ZonesListParams = {}) => ({
        url: `/zones`,
        params: {
          warehouseId,
          page,
          pageSize,
          search,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'Zone' as const, id })),
              { type: 'Zone', id: 'LIST' },
            ]
          : [{ type: 'Zone', id: 'LIST' }],
    }),

    getZoneById: builder.query<ZoneDetailResponse, string>({
      query: (id: string) => `/zones/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Zone', id }],
    }),

    createZone: builder.mutation<ZoneDetailResponse, CreateZoneRequest>({
      query: (body: CreateZoneRequest) => ({
        url: `/zones`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'Zone', id: 'LIST' }],
    }),

    updateZone: builder.mutation<ZoneDetailResponse, { id: string; data: UpdateZoneRequest }>({
      query: ({ id, data }: { id: string; data: UpdateZoneRequest }) => ({
        url: `/zones/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Zone', id },
        { type: 'Zone', id: 'LIST' },
      ],
    }),

    deleteZone: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/zones/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Zone', id },
        { type: 'Zone', id: 'LIST' },
      ],
    }),
  }),
});

export const {
  useGetZonesQuery,
  useLazyGetZonesQuery,
  useGetZoneByIdQuery,
  useLazyGetZoneByIdQuery,
  useCreateZoneMutation,
  useUpdateZoneMutation,
  useDeleteZoneMutation,
} = zonesApi;
