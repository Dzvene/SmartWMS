/**
 * Configuration API Slice
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  // Barcode Prefixes
  BarcodePrefixListResponse,
  BarcodePrefixResponse,
  BarcodePrefixFilters,
  CreateBarcodePrefixRequest,
  UpdateBarcodePrefixRequest,
  // Reason Codes
  ReasonCodeListResponse,
  ReasonCodeResponse,
  ReasonCodeFilters,
  CreateReasonCodeRequest,
  UpdateReasonCodeRequest,
  // Number Sequences
  NumberSequenceListResponse,
  NumberSequenceResponse,
  NumberSequenceFilters,
  CreateNumberSequenceRequest,
  UpdateNumberSequenceRequest,
  // System Settings
  SystemSettingListResponse,
  SystemSettingResponse,
  SystemSettingFilters,
  UpdateSystemSettingRequest,
} from './configuration.types';

export const configurationApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Barcode Prefixes
    // ========================================================================

    getBarcodePrefixes: builder.query<BarcodePrefixListResponse, BarcodePrefixFilters | void>({
      query: (params) => ({
        url: `/configuration/barcode-prefixes`,
        params: params || {},
      }),
      providesTags: ['BarcodePrefixes'],
    }),

    getBarcodePrefixById: builder.query<BarcodePrefixResponse, string>({
      query: (id) => `/configuration/barcode-prefixes/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'BarcodePrefixes', id }],
    }),

    createBarcodePrefix: builder.mutation<BarcodePrefixResponse, CreateBarcodePrefixRequest>({
      query: (body) => ({
        url: `/configuration/barcode-prefixes`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['BarcodePrefixes'],
    }),

    updateBarcodePrefix: builder.mutation<BarcodePrefixResponse, { id: string; data: UpdateBarcodePrefixRequest }>({
      query: ({ id, data }) => ({
        url: `/configuration/barcode-prefixes/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['BarcodePrefixes'],
    }),

    deleteBarcodePrefix: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/configuration/barcode-prefixes/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['BarcodePrefixes'],
    }),

    // ========================================================================
    // Reason Codes
    // ========================================================================

    getReasonCodes: builder.query<ReasonCodeListResponse, ReasonCodeFilters | void>({
      query: (params) => ({
        url: `/configuration/reason-codes`,
        params: params || {},
      }),
      providesTags: ['ReasonCodes'],
    }),

    getReasonCodeById: builder.query<ReasonCodeResponse, string>({
      query: (id) => `/configuration/reason-codes/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'ReasonCodes', id }],
    }),

    createReasonCode: builder.mutation<ReasonCodeResponse, CreateReasonCodeRequest>({
      query: (body) => ({
        url: `/configuration/reason-codes`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['ReasonCodes'],
    }),

    updateReasonCode: builder.mutation<ReasonCodeResponse, { id: string; data: UpdateReasonCodeRequest }>({
      query: ({ id, data }) => ({
        url: `/configuration/reason-codes/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['ReasonCodes'],
    }),

    deleteReasonCode: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/configuration/reason-codes/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['ReasonCodes'],
    }),

    // ========================================================================
    // Number Sequences
    // ========================================================================

    getNumberSequences: builder.query<NumberSequenceListResponse, NumberSequenceFilters | void>({
      query: (params) => ({
        url: `/configuration/number-sequences`,
        params: params || {},
      }),
      providesTags: ['NumberSequences'],
    }),

    getNumberSequenceById: builder.query<NumberSequenceResponse, string>({
      query: (id) => `/configuration/number-sequences/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'NumberSequences', id }],
    }),

    createNumberSequence: builder.mutation<NumberSequenceResponse, CreateNumberSequenceRequest>({
      query: (body) => ({
        url: `/configuration/number-sequences`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['NumberSequences'],
    }),

    updateNumberSequence: builder.mutation<NumberSequenceResponse, { id: string; data: UpdateNumberSequenceRequest }>({
      query: ({ id, data }) => ({
        url: `/configuration/number-sequences/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['NumberSequences'],
    }),

    deleteNumberSequence: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/configuration/number-sequences/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['NumberSequences'],
    }),

    getNextNumber: builder.mutation<ApiResponse<string>, string>({
      query: (sequenceName) => ({
        url: `/configuration/number-sequences/${sequenceName}/next`,
        method: 'POST',
      }),
      invalidatesTags: ['NumberSequences'],
    }),

    // ========================================================================
    // System Settings
    // ========================================================================

    getSystemSettings: builder.query<SystemSettingListResponse, SystemSettingFilters | void>({
      query: (params) => ({
        url: `/configuration/settings`,
        params: params || {},
      }),
      providesTags: ['SystemSettings'],
    }),

    getSystemSettingByKey: builder.query<SystemSettingResponse, { category: string; key: string }>({
      query: ({ category, key }) => `/configuration/settings/${category}/${key}`,
      providesTags: (_r, _e, { category, key }) => [{ type: 'SystemSettings', id: `${category}-${key}` }],
    }),

    updateSystemSetting: builder.mutation<
      SystemSettingResponse,
      { category: string; key: string; data: UpdateSystemSettingRequest }
    >({
      query: ({ category, key, data }) => ({
        url: `/configuration/settings/${category}/${key}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['SystemSettings'],
    }),
  }),
});

export const {
  // Barcode Prefixes
  useGetBarcodePrefixesQuery,
  useGetBarcodePrefixByIdQuery,
  useCreateBarcodePrefixMutation,
  useUpdateBarcodePrefixMutation,
  useDeleteBarcodePrefixMutation,
  // Reason Codes
  useGetReasonCodesQuery,
  useGetReasonCodeByIdQuery,
  useCreateReasonCodeMutation,
  useUpdateReasonCodeMutation,
  useDeleteReasonCodeMutation,
  // Number Sequences
  useGetNumberSequencesQuery,
  useGetNumberSequenceByIdQuery,
  useCreateNumberSequenceMutation,
  useUpdateNumberSequenceMutation,
  useDeleteNumberSequenceMutation,
  useGetNextNumberMutation,
  // System Settings
  useGetSystemSettingsQuery,
  useGetSystemSettingByKeyQuery,
  useUpdateSystemSettingMutation,
} = configurationApi;
