/**
 * Sites API Slice
 * RTK Query endpoints for Sites module
 *
 * Endpoints:
 * - /sites - Site CRUD
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type {
  SitesListParams,
  SitesListResponse,
  SiteDetailResponse,
  CreateSiteRequest,
  UpdateSiteRequest,
} from './sites.types';
import type { ApiResponse } from '@/api/types';

export const sitesApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Sites CRUD
    // ========================================================================

    getSites: builder.query<SitesListResponse, SitesListParams | void>({
      query: ({
        page = 1,
        pageSize = 100,
        search,
        isActive,
      }: SitesListParams = {}) => ({
        url: `/sites`,
        params: {
          page,
          pageSize,
          search,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'Site' as const, id })),
              { type: 'Site', id: 'LIST' },
            ]
          : [{ type: 'Site', id: 'LIST' }],
    }),

    getSiteById: builder.query<SiteDetailResponse, string>({
      query: (id: string) => `/sites/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Site', id }],
    }),

    createSite: builder.mutation<SiteDetailResponse, CreateSiteRequest>({
      query: (body: CreateSiteRequest) => ({
        url: `/sites`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'Site', id: 'LIST' }],
    }),

    updateSite: builder.mutation<SiteDetailResponse, { id: string; data: UpdateSiteRequest }>({
      query: ({ id, data }: { id: string; data: UpdateSiteRequest }) => ({
        url: `/sites/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Site', id },
        { type: 'Site', id: 'LIST' },
      ],
    }),

    deleteSite: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/sites/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Site', id },
        { type: 'Site', id: 'LIST' },
      ],
    }),
  }),
});

export const {
  useGetSitesQuery,
  useLazyGetSitesQuery,
  useGetSiteByIdQuery,
  useLazyGetSiteByIdQuery,
  useCreateSiteMutation,
  useUpdateSiteMutation,
  useDeleteSiteMutation,
} = sitesApi;
