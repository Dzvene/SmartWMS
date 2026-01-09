/**
 * Carriers API Slice
 * RTK Query endpoints for Carriers module
 */

import { baseApi } from '@/api/baseApi';
import type {
  CarrierFilters,
  CarrierResponse,
  CarrierListResponse,
  CarrierServiceResponse,
  CreateCarrierRequest,
  UpdateCarrierRequest,
  CreateCarrierServiceRequest,
  UpdateCarrierServiceRequest,
} from './carriers.types';

export const carriersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Carriers CRUD
    // ========================================================================

    getCarriers: builder.query<CarrierListResponse, CarrierFilters | void>({
      query: (params) => ({
        url: '/carriers',
        params: params || {},
      }),
      providesTags: ['Carriers'],
    }),

    getCarrierById: builder.query<CarrierResponse, string>({
      query: (id) => `/carriers/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Carriers', id }],
    }),

    createCarrier: builder.mutation<CarrierResponse, CreateCarrierRequest>({
      query: (body) => ({
        url: '/carriers',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Carriers'],
    }),

    updateCarrier: builder.mutation<CarrierResponse, { id: string; body: UpdateCarrierRequest }>({
      query: ({ id, body }) => ({
        url: `/carriers/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Carriers', id }, 'Carriers'],
    }),

    deleteCarrier: builder.mutation<void, string>({
      query: (id) => ({
        url: `/carriers/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Carriers'],
    }),

    // ========================================================================
    // Carrier Services
    // ========================================================================

    getCarrierServices: builder.query<CarrierServiceResponse[], string>({
      query: (carrierId) => `/carriers/${carrierId}/services`,
      providesTags: (_result, _error, carrierId) => [{ type: 'Carriers', id: carrierId }],
    }),

    createCarrierService: builder.mutation<CarrierServiceResponse, { carrierId: string; body: CreateCarrierServiceRequest }>({
      query: ({ carrierId, body }) => ({
        url: `/carriers/${carrierId}/services`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { carrierId }) => [{ type: 'Carriers', id: carrierId }],
    }),

    updateCarrierService: builder.mutation<CarrierServiceResponse, { carrierId: string; serviceId: string; body: UpdateCarrierServiceRequest }>({
      query: ({ carrierId, serviceId, body }) => ({
        url: `/carriers/${carrierId}/services/${serviceId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { carrierId }) => [{ type: 'Carriers', id: carrierId }],
    }),

    deleteCarrierService: builder.mutation<void, { carrierId: string; serviceId: string }>({
      query: ({ carrierId, serviceId }) => ({
        url: `/carriers/${carrierId}/services/${serviceId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, { carrierId }) => [{ type: 'Carriers', id: carrierId }],
    }),

    // ========================================================================
    // Carrier Lookup
    // ========================================================================

    getActiveCarriers: builder.query<CarrierListResponse, void>({
      query: () => '/carriers/active',
      providesTags: ['Carriers'],
    }),
  }),
});

// Export hooks
export const {
  // Carriers
  useGetCarriersQuery,
  useLazyGetCarriersQuery,
  useGetCarrierByIdQuery,
  useLazyGetCarrierByIdQuery,
  useCreateCarrierMutation,
  useUpdateCarrierMutation,
  useDeleteCarrierMutation,

  // Services
  useGetCarrierServicesQuery,
  useLazyGetCarrierServicesQuery,
  useCreateCarrierServiceMutation,
  useUpdateCarrierServiceMutation,
  useDeleteCarrierServiceMutation,

  // Lookup
  useGetActiveCarriersQuery,
  useLazyGetActiveCarriersQuery,
} = carriersApi;
