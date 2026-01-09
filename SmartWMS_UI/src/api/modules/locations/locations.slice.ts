/**
 * Locations API Slice
 * RTK Query endpoints for Locations module
 *
 * Endpoints:
 * - /locations - Location CRUD
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type {
  LocationsListParams,
  LocationsListResponse,
  LocationDetailResponse,
  CreateLocationRequest,
  UpdateLocationRequest,
} from './locations.types';
import type { ApiResponse } from '@/api/types';

export const locationsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Locations CRUD
    // ========================================================================

    getLocations: builder.query<LocationsListResponse, LocationsListParams | void>({
      query: ({
        warehouseId,
        zoneId,
        page = 1,
        pageSize = 100,
        search,
        isActive,
      }: LocationsListParams = {}) => ({
        url: `/locations`,
        params: {
          warehouseId,
          zoneId,
          page,
          pageSize,
          search,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'Location' as const, id })),
              { type: 'Location', id: 'LIST' },
            ]
          : [{ type: 'Location', id: 'LIST' }],
    }),

    getLocationById: builder.query<LocationDetailResponse, string>({
      query: (id: string) => `/locations/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Location', id }],
    }),

    createLocation: builder.mutation<LocationDetailResponse, CreateLocationRequest>({
      query: (body: CreateLocationRequest) => ({
        url: `/locations`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'Location', id: 'LIST' }, { type: 'Zone', id: 'LIST' }],
    }),

    updateLocation: builder.mutation<LocationDetailResponse, { id: string; data: UpdateLocationRequest }>({
      query: ({ id, data }: { id: string; data: UpdateLocationRequest }) => ({
        url: `/locations/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Location', id },
        { type: 'Location', id: 'LIST' },
        { type: 'Zone', id: 'LIST' },
      ],
    }),

    deleteLocation: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/locations/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Location', id },
        { type: 'Location', id: 'LIST' },
        { type: 'Zone', id: 'LIST' },
      ],
    }),
  }),
});

export const {
  useGetLocationsQuery,
  useLazyGetLocationsQuery,
  useGetLocationByIdQuery,
  useLazyGetLocationByIdQuery,
  useCreateLocationMutation,
  useUpdateLocationMutation,
  useDeleteLocationMutation,
} = locationsApi;
