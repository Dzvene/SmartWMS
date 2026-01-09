/**
 * Warehouses API Slice
 * RTK Query endpoints for Warehouses module
 *
 * Endpoints:
 * - /warehouses - Warehouse CRUD
 * - /warehouses/options - Warehouse options for dropdowns
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type {
  WarehousesListParams,
  WarehousesListResponse,
  WarehouseOptionsResponse,
  WarehouseDetailResponse,
  CreateWarehouseRequest,
  UpdateWarehouseRequest,
  WarehouseOptionsParams,
} from './warehouses.types';
import type { ApiResponse } from '@/api/types';

export const warehousesApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Warehouses CRUD
    // ========================================================================

    getWarehouses: builder.query<WarehousesListResponse, WarehousesListParams | void>({
      query: ({
        siteId,
        page = 1,
        pageSize = 100,
        search,
        isActive,
      }: WarehousesListParams = {}) => ({
        url: `/warehouses`,
        params: {
          siteId,
          page,
          pageSize,
          search,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'Warehouse' as const, id })),
              { type: 'Warehouse', id: 'LIST' },
            ]
          : [{ type: 'Warehouse', id: 'LIST' }],
    }),

    getWarehouseOptions: builder.query<WarehouseOptionsResponse, WarehouseOptionsParams | void>({
      query: ({ siteId }: WarehouseOptionsParams = {}) => ({
        url: `/warehouses/options`,
        params: siteId ? { siteId } : undefined,
      }),
      providesTags: [{ type: 'Warehouse', id: 'OPTIONS' }],
    }),

    getWarehouseById: builder.query<WarehouseDetailResponse, string>({
      query: (id: string) => `/warehouses/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Warehouse', id }],
    }),

    createWarehouse: builder.mutation<WarehouseDetailResponse, CreateWarehouseRequest>({
      query: (body: CreateWarehouseRequest) => ({
        url: `/warehouses`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'Warehouse', id: 'LIST' }, { type: 'Warehouse', id: 'OPTIONS' }],
    }),

    updateWarehouse: builder.mutation<WarehouseDetailResponse, { id: string; data: UpdateWarehouseRequest }>({
      query: ({ id, data }: { id: string; data: UpdateWarehouseRequest }) => ({
        url: `/warehouses/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Warehouse', id },
        { type: 'Warehouse', id: 'LIST' },
      ],
    }),

    deleteWarehouse: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/warehouses/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Warehouse', id },
        { type: 'Warehouse', id: 'LIST' },
        { type: 'Warehouse', id: 'OPTIONS' },
      ],
    }),
  }),
});

export const {
  useGetWarehousesQuery,
  useLazyGetWarehousesQuery,
  useGetWarehouseOptionsQuery,
  useLazyGetWarehouseOptionsQuery,
  useGetWarehouseByIdQuery,
  useLazyGetWarehouseByIdQuery,
  useCreateWarehouseMutation,
  useUpdateWarehouseMutation,
  useDeleteWarehouseMutation,
} = warehousesApi;
